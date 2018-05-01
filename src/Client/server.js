import { createApp } from "./Client.fsproj";

export default (context) => {
    const app = createApp(context);

    return new Promise((resolve, reject) => {
        app.router.push(context.url);

        app.router.onReady(() => {
            const matchedComponents = app.router.getMatchedComponents();
            if (!matchedComponents.length) {
                return reject({ message: "Invalid route", code: 404 });
            }

            Promise.all(matchedComponents.map((component) => {
                const options = component.options;
                if (options && options.asyncData) {
                    return options.asyncData(app.store, app.router.currentRoute);
                }
            })).then(() => {
                context.state = app.store.state;
                resolve(app.main);
            }).catch(reject);
        });
    });
};
