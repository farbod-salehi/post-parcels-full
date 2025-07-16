import { createStore } from "vuex";

const store = createStore({
  state() {
    return {
      authInfo: null,
    };
  },
  mutations: {
    // we change state by mutations methods, thus all components do it in the same way.
    // for asynchronous code, we should use 'actions'
    setAuthInfo(state, authInfoObj) {
      // 'state' parameter will be passed automatically. Other parameters are optional.
      localStorage.setItem("authInfo", JSON.stringify(authInfoObj));
      state.authInfo = authInfoObj;
    },
    removeAuthInfo(state) {
      localStorage.removeItem("authInfo");
      state.authInfo = null;
    },
    tryLogin(state) {
      const authInfoStr = localStorage.getItem("authInfo");
      if (authInfoStr) {
        state.authInfo = JSON.parse(authInfoStr);
      } else {
        state.authInfo = null;
      }
    },
  },
  getters: {
    // we use this, for reading state in the same way in different components : $this.$store.getters.getAuthInfo
    getAuthInfo(state /*, getters*/) {
      // 'gettters': access to other getters.
      return state.authInfo;
    },
    userIsAuthenticated(state) {
      return state.authInfo !== null;
    },
  },
});

export default store;
