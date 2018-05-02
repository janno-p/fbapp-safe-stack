const path = require("path");
const webpack = require("webpack");
const fableUtils = require("fable-utils");
const nodeExternals = require("webpack-node-externals");

const UglifyJsPlugin = require("uglifyjs-webpack-plugin");
const VueSsrServerPlugin = require("vue-server-renderer/server-plugin");

function resolve(filePath) {
    return path.join(__dirname, filePath)
}

const babelOptions = fableUtils.resolveBabelOptions({
    presets: [
        [
            "env",
            {
                "targets": {
                    "browsers": ["last 2 versions"]
                },
                "modules": false
            }
        ]
    ],
    plugins: ["transform-runtime"]
});

const isProduction = process.argv.indexOf("-p") >= 0;
const mode = isProduction ? "production" : "development";

function createConfig({ serverSide }) {
    console.log(`Bundling for ${mode} (${serverSide ? "SSR" : "Client"})...`);

    const plugins = isProduction ? [] : [
        new webpack.NamedModulesPlugin()
    ];
    
    let externals;
    let libraryTarget;
    
    if (serverSide) {
        plugins.push(new webpack.DefinePlugin({
            "process.env": {
                NODE_ENV: isProduction ? "\"production\"" : "\"development\"",
                VUE_ENV: "\"server\""
            }
        }));
        plugins.push(new UglifyJsPlugin());
        plugins.push(new VueSsrServerPlugin());
        
        externals = nodeExternals();
        libraryTarget = "commonjs2"
    }

    return {
        mode: mode,
        
        devtool: "source-map",
        
        entry: {
            bundle: serverSide ? resolve("./server.js") : resolve("./client.js")
        },
        
        externals,
        
        output: {
            path: resolve('./public/'),
            publicPath: "/public/",
            filename: "bundle.js",
            libraryTarget
        },
        
        resolve: {
            modules: [resolve("../../node_modules/")]
        },
        
        module: {
            rules: [
                {
                    test: /\.fs(x|proj)?$/,
                    use: {
                        loader: "fable-loader",
                        options: {
                            babel: babelOptions,
                            define: isProduction ? [] : ["DEBUG"]
                        }
                    }
                },
                {
                    test: /\.js$/,
                    exclude: /node_modules/,
                    use: {
                        loader: 'babel-loader',
                        options: babelOptions
                    },
                },
                {
                    test: /\.json?$/,
                    loader: "json-loader"
                },
                {
                    test: /\.scss$/,
                    use: [
                        "style-loader",
                        "css-loader",
                        "sass-loader"
                    ]
                }
            ]
        },
        
        plugins,
        
        target: serverSide ? "node" : "web"
    };
}

module.exports = [
    createConfig({ serverSide: true }),
    createConfig({ serverSide: false })
];
