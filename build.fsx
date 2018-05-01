#r @"packages/build/FAKE/tools/FakeLib.dll"
#r @"packages/build/BouncyCastle/lib/BouncyCastle.Crypto"

open Fake
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

let serverPath = "./src/Server" |> FullName
let clientPath = "./src/Client" |> FullName
let deployDir = "./deploy" |> FullName

let platformTool tool winTool =
    let tool = if isUnix then tool else winTool
    tool
    |> ProcessHelper.tryFindFileOnPath
    |> function Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = platformTool "node" "node.exe"
let yarnTool = platformTool "yarn" "yarn.cmd"

let dotnetcliVersion = DotNetCli.GetDotNetSDKVersionFromGlobalJson()
let mutable dotnetCli = "dotnet"

let run cmd args workingDir =
    let result =
        ExecProcess (fun info ->
            info.FileName <- cmd
            info.WorkingDirectory <- workingDir
            info.Arguments <- args) TimeSpan.MaxValue
    if result <> 0 then failwithf "'%s %s' failed" cmd args

Target "Clean" (fun _ -> 
    CleanDirs [deployDir]
)

Target "InstallDotNetCore" (fun _ ->
    dotnetCli <- DotNetCli.InstallDotNetSDK dotnetcliVersion
)

Target "InstallClient" (fun _ ->
    printfn "Node version:"
    run nodeTool "--version" __SOURCE_DIRECTORY__
    printfn "Yarn version:"
    run yarnTool "--version" __SOURCE_DIRECTORY__
    run yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
    run dotnetCli "restore" clientPath
)

Target "RestoreServer" (fun () -> 
    run dotnetCli "restore" serverPath
)

Target "Build" (fun () ->
    run dotnetCli "build" serverPath
    run dotnetCli "fable webpack -- -p" clientPath
)

Target "Run" (fun () ->
    let server = async {
        setEnvironVar "ASPNETCORE_ENVIRONMENT" "Development"
        run dotnetCli "watch run" serverPath
    }
    let client = async {
        run dotnetCli "fable start" clientPath
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

Target "GenerateCertificate" (fun () ->
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
    ==> "InstallDotNetCore"
    ==> "InstallClient"
    ==> "Build"

"InstallClient"
    ==> "RestoreServer"
    ==> "Run"

RunTargetOrDefault "Build"