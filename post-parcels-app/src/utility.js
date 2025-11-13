import { createToaster } from "@meforma/vue-toaster";
import store from "./store.js";
import axios from "axios";

export const baseAPI =
  process.env.NODE_ENV === "development"
    ? "https://localhost:7018"
    : "http://10.9.35.45:81";

export function sendRequest(method, url, paramsObject, jwt = null) {
  return new Promise((resolve, reject) => {
    fetch(`${baseAPI}${url}`, {
      method: method,
      headers: {
        "Content-Type": "application/json",
        Authorization: jwt ? `Bearer ${jwt}` : "",
      },
      body: JSON.stringify(paramsObject),
    })
      .then(async function (response) {
        if (response.status === 204) {
          // it means nocontent response, which is not a json response
          return {};
        }
        if (!response.ok) {
          const errorData = await response.json(); // Parse JSON response
          throw {
            status: response.status,
            content: JSON.stringify(errorData),
          };
        }

        return response.json(); // will return data as json. it will be catched in the next 'then()'
      })
      .then(function (data) {
        resolve(data);
      })
      .catch(function (errorObj) {
        reject(errorObj);
      });
  });
}

export async function sendRequestByFormData(url, formData, jwt = null) {
  const response = await axios.post(`${baseAPI}${url}`, formData, {
    headers: {
      "Content-Type": "multipart/form-data",
      Authorization: jwt ? `Bearer ${jwt}` : "",
    },
  });

  return response.data;
}

export function requestErrorHandling(error, router) {
  if ("status" in error === false) {
    return ["خطای نامشحصی اتفاق افتاده است."];
  }
  let act = JSON.parse(error.content).act;
  if (error.status === 401 && act === "login") {
    store.commit("removeAuthInfo");
    router.replace({ name: "login" });
  }

  if (error.status === 403 && act === "message") {
    showToast("error", "شما مجوز دسترسی به این بخش را ندارید.");
  }

  return JSON.parse(error.content)?.error?.split("|") ?? [];
}

export function showToast(typeName, message) {
  createToaster({
    type: typeName,
    position: "top",
    duration: 3000,
    dismissible: true,
  }).show(`<span style="font-family:Vazir"> ${message} </span>`);
}

export function getRoutePath(router, routeName, routeParamsObject) {
  const result = router.resolve({
    name: routeName,
    params: routeParamsObject || {},
  });

  return result.href;
}

export function openParcelsReport(parcelId) {
  const url = new URL(
    location.origin +
      `/reports/parcelpackets.html?requestAPI=${baseAPI}/api/parcels/${parcelId}`
  );
  window.open(url.toString(), "_blank");
}
