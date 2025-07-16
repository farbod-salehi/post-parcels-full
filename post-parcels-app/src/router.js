import { createRouter, createWebHistory } from "vue-router";
import { showToast } from "./utility";
import store from "./store.js";
import ParcelItemTrackingForm from "./components/parcel/ParcelItemTrackingForm.vue";
import ParcelStatisticForm from "./components/parcel/ParcelStatisticForm.vue";

const LoginForm = () => import("./components/user/LoginForm.vue");
const UserAddForm = () => import("./components/user/UserAddForm.vue");
const UserEditForm = () => import("./components/user/UserEditForm.vue");
const UserListForm = () => import("./components/user/UserListForm.vue");
const UnitItemForm = () => import("./components/unit/UnitItemForm.vue");
const UnitListForm = () => import("./components/unit/UnitListForm.vue");
const ParcelItemForm = () => import("./components/parcel/ParcelItemForm.vue");
const ParcelItemDocumentForm = () =>
  import("./components/parcel/ParcelItemDocumentForm.vue");
const ParcelListForm = () => import("./components/parcel/ParcelListForm.vue");
const ChangeMyPasswordForm = () =>
  import("./components/user/ChangeMyPasswordForm.vue");
const DashboardForm = () => import("./components/DashboardForm.vue");
const NotFoundPage = () => import("./components/NotFound.vue");

const router = createRouter({
  routes: [
    {
      name: "root",
      path: "/",
      redirect: "/parcels/item",
      meta: { needsAuth: true },
    },
    {
      name: "dashboard",
      path: "/dashboard",
      component: DashboardForm,
      meta: { needsAuth: true },
    },
    { name: "login", path: "/login", component: LoginForm },
    { name: "userAdd", path: "/users/add", component: UserAddForm },
    {
      name: "userEdit",
      path: "/users/edit/:id",
      component: UserEditForm,
      props: true,
      meta: { needsAuth: true, validRoles: [1] },
    },
    {
      name: "userList",
      path: "/users/list",
      component: UserListForm,
      meta: { needsAuth: true, validRoles: [1] },
    },
    {
      name: "changeMyPassword",
      path: "/users/me/password",
      component: ChangeMyPasswordForm,
    },
    {
      name: "unitList",
      path: "/units/list/:parentId?",
      props: true,
      component: UnitListForm,
      meta: { needsAuth: true, validRoles: [1, 2] },
    },
    {
      name: "unitItem",
      path: "/units/item/:editId?",
      props: true,
      component: UnitItemForm,
      meta: { needsAuth: true, validRoles: [1, 2] },
    },

    {
      name: "parcelItem",
      path: "/parcels/item/:editId?",
      props: true,
      component: ParcelItemForm,
      meta: { needsAuth: true },
    },
    {
      name: "parcelItemDocuments",
      path: "/parcels/item/:editId/documents",
      props: true,
      component: ParcelItemDocumentForm,
      meta: { needsAuth: true },
    },
    {
      name: "parcelItemsTracking",
      path: "/parcels/items/tracking",
      props: true,
      component: ParcelItemTrackingForm,
      meta: { needsAuth: true },
    },
    {
      name: "parcelStatistic",
      path: "/parcels/statistic",
      props: true,
      component: ParcelStatisticForm,
      meta: { needsAuth: true },
    },
    {
      name: "parcelList",
      path: "/parcels/list",
      props: true,
      component: ParcelListForm,
      meta: { needsAuth: true },
    },
    { path: "/:notFound(.*)", component: NotFoundPage },
  ],
  history: createWebHistory(),
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition;
    } else return { left: 0, top: 0 };
  },
});

router.beforeEach(function (to, from, next) {
  if (to.meta.needsAuth && !store.getters.userIsAuthenticated) {
    return next({ name: "login" });
  }

  if (
    to.meta.validRoles &&
    to.meta.validRoles.includes(store.getters.getAuthInfo.userRole) === false
  ) {
    showToast("error", "شما مجوز دسترسی به این بخش را ندارید.");
    return false;
  }

  next();
});

export default router;
