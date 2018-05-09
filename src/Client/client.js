import NProgress from "nprogress";
import Vue, { ComponentOptions } from "vue";

import "noty/src/noty.scss";
import "noty/src/themes/mint.scss";
import "./Styles/nprogress.scss"
import "./Styles/dashboard.scss"

import { createApp } from "./Client.fsproj";

const app = createApp({ baseUrl: window.__SERVER_URL__ });

if (window.__INITIAL_STATE__) {
    app.store.replaceState(window.__INITIAL_STATE__);
}

app.router.onReady(() => {
    app.router.beforeResolve((to, from, next) => {
        const toComponents = app.router.getMatchedComponents(to);
        const fromComponents = app.router.getMatchedComponents(from);

        let diffed = false;
        const activated = toComponents.filter((component, i) => {
            return diffed || (diffed = (fromComponents[i] !== component));
        });

        if (!activated.length) {
            return next();
        }

        NProgress.start();

        Promise.all(activated.map((component) => {
            const options = component.options;
            if (options && options.asyncData) {
                return options.asyncData(app.store, to);
            }
        })).then(() => {
            NProgress.done();
            next();
        }).catch(next);
    });

    app.main.$mount("#app");
});
