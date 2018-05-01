process.env.VUE_ENV = "server";

const path = require("path");
const createServerRenderer = require("aspnet-prerendering").createServerRenderer;
const createBundleRenderer = require("vue-server-renderer").createBundleRenderer;

const bundleRenderer = createBundleRenderer(path.join(__dirname, "public/vue-ssr-server-bundle.json"), {
    runInNewContext: false
});

module.exports = createServerRenderer((params) => {
    return new Promise((resolve, reject) => {
        const context = {
            baseUrl: `http://${params.data.request.host}`,
            isAuthenticated: params.data.isAuthenticated,
            request: params.data.request,
            state: {},
            url: params.url
        };

        bundleRenderer.renderToString(context, (err, resultHtml) => {
            if (err) {
                reject(err.message);
                return;
            }

            resolve({
                globals: {
                    __INITIAL_STATE__: context.state,
                    __SERVER_URL__: context.baseUrl
                },
                html: resultHtml
            });
        });
    });
});
