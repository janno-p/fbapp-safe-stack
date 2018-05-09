#r @"packages/build/FAKE/tools/FakeLib.dll"
#r @"packages/build/BouncyCastle/lib/BouncyCastle.Crypto"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Org.BouncyCastle.Crypto
open Org.BouncyCastle.Crypto.Generators
open Org.BouncyCastle.Crypto.Operators
open Org.BouncyCastle.Crypto.Prng
open Org.BouncyCastle.Math
open Org.BouncyCastle.Pkcs
open Org.BouncyCastle.Security
open Org.BouncyCastle.Utilities
open Org.BouncyCastle.X509
open System
open System.IO

let [<Literal>] ApplicationName = "FbApp"
let [<Literal>] KeyStrength = 2048
let [<Literal>] SignatureAlgorithm = "SHA256WithRSA"
let [<Literal>] SubjectName = "CN=localhost"

let srcDir = __SOURCE_DIRECTORY__ </> "src"
let certificatePath = srcDir </> (sprintf "%s.pfx" ApplicationName)

let serverPath = "./src/Server" |> Path.getFullName
let clientPath = "./src/Client" |> Path.getFullName
let deployDir = "./deploy" |> Path.getFullName

let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    tool
    |> Process.tryFindFileOnPath
    |> function Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = platformTool "node" "node.exe"
let yarnTool = platformTool "yarn" "yarn.cmd"

let dotnetcliVersion = DotNet.getSDKVersionFromGlobalJson()

let install =
    lazy DotNet.install (fun opt -> { opt with Version = DotNet.Version dotnetcliVersion})

let inline withWorkingDir wd =
    DotNet.Options.lift install.Value
    >> DotNet.Options.withWorkingDirectory wd

let run cmd args workingDir =
    let result =
        Process.execSimple
            (fun info ->
                { info with
                    FileName = cmd
                    WorkingDirectory = workingDir
                    Arguments = args })
            TimeSpan.MaxValue
    if result <> 0 then failwithf "'%s %s' failed" cmd args

Target.create "Clean" (fun _ -> 
    Shell.cleanDirs [deployDir]
)

Target.create "InstallClient" (fun _ ->
    printfn "Node version:"
    run nodeTool "--version" __SOURCE_DIRECTORY__
    printfn "Yarn version:"
    run yarnTool "--version" __SOURCE_DIRECTORY__
    run yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
    DotNet.restore (withWorkingDir clientPath) ""
)

Target.create "RestoreServer" (fun _ ->
    DotNet.restore (withWorkingDir serverPath) ""
)

Target.create "Build" (fun _ ->
    DotNet.build (withWorkingDir serverPath) ""
    let result = DotNet.exec (withWorkingDir clientPath) "fable" "webpack -- -p"
    if not result.OK then
        failwithf "'fable webpack' failed with errors %A." result.Errors
)

Target.create "Run" (fun _ ->
    let server = async {
        Environment.setEnvironVar "ASPNETCORE_ENVIRONMENT" "Development"
        let result = DotNet.exec (withWorkingDir serverPath) "watch" "run"
        if not result.OK then
            failwithf "'watch run' failed with errors %A." result.Errors
    }
    let client = async {
        let result = DotNet.exec (withWorkingDir clientPath) "fable" "start"
        if not result.OK then
            failwithf "'fable start' failed with errors %A." result.Errors
    }
    let browser = async {
        Threading.Thread.Sleep 5000
        Diagnostics.Process.Start "https://localhost:5001" |> ignore
    }

    [ server; client; browser]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "GenerateCertificate" (fun _ ->
    let randomGenerator = CryptoApiRandomGenerator()
    let random = SecureRandom(randomGenerator)

    let certificateGenerator = X509V3CertificateGenerator()

    let serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random)
    certificateGenerator.SetSerialNumber(serialNumber)

    let subjectDN = Org.BouncyCastle.Asn1.X509.X509Name(SubjectName)
    let issuerDN = subjectDN
    certificateGenerator.SetIssuerDN(issuerDN)
    certificateGenerator.SetSubjectDN(subjectDN)

    let notBefore = DateTime.UtcNow.Date
    let notAfter = notBefore.AddYears(2)

    certificateGenerator.SetNotBefore(notBefore)
    certificateGenerator.SetNotAfter(notAfter)

    let keyGenerationParameters = KeyGenerationParameters(random, KeyStrength)
    let keyPairGenerator = RsaKeyPairGenerator()
    keyPairGenerator.Init(keyGenerationParameters)
    let subjectKeyPair = keyPairGenerator.GenerateKeyPair()

    certificateGenerator.SetPublicKey(subjectKeyPair.Public)

    let issuerKeyPair = subjectKeyPair
    let certificate = certificateGenerator.Generate(Asn1SignatureFactory(SignatureAlgorithm, issuerKeyPair.Private, random))

    let store = Pkcs12Store()
    let friendlyName = certificate.SubjectDN.ToString()

    let certificateEntry = X509CertificateEntry(certificate)
    store.SetCertificateEntry(friendlyName, certificateEntry)

    store.SetKeyEntry(friendlyName, AsymmetricKeyEntry(subjectKeyPair.Private), [| certificateEntry |])

    use stream = new MemoryStream()
    store.Save(stream, [||], random)

    stream.Position <- 0L
    File.WriteAllBytes(certificatePath, stream.ToArray())
)

"Clean"
    ==> "InstallClient"
    ==> "Build"

"InstallClient"
    ==> "RestoreServer"
    ==> "Run"

Target.runOrDefault "Build"
