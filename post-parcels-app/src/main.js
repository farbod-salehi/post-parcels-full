import { createApp } from "vue";
import App from "./App.vue";

import router from "./router.js";
import store from "./store.js";

import "./assets/css/bootstrap.rtl.min.css";
import "./assets/js/bootstrap.bundle.min.js";

import "./assets/css/styles.css";

// Import all icons
import { BootstrapIconsPlugin } from "bootstrap-icons-vue";

const app = createApp(App);

app.use(store);
app.use(router);

app.use(BootstrapIconsPlugin);

app.mount("#app");
