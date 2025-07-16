<template>
    <div class="alert alert-secondary form-title-container" role="alert">
        <div class="d-flex justify-content-between">
            <div>
                تغییر کلمه عبور
            </div>
            <div>


            </div>
        </div>

    </div>
    <div id="form-container">
        <div v-show="messages.length > 0" class="alert alert-danger alert-dismissible fade show" style="margin-bottom:50px"
            role="alert">
            <div v-for="mes of messages" :key="mes" class="mt-1">
                - {{ mes }}
            </div>
            <button type="button" class="btn-close" aria-label="Close" @click="clearMessages"></button>
        </div>
        <form @submit.prevent="submit">
            <div class="row">
                <div class="col">
                    <label for="inputCurrentPassword"> کلمه عبور فعلی </label>
                    <input type="password" class="form-control ltr" id="inputCurrentPassword" v-model="state.currentPassword" />
                    <div v-if="v$.currentPassword.$error" class="form-text text-danger rtl input-validation-text">
                         کلمه عبور فعلی را وارد نمایید.
                    </div>

                </div>
            </div>
            <div class="row mt-2">
                <div class="col">
                    <label for="inputNewPassword"> کلمه عبور جدید</label>
                    <input type="password" class="form-control ltr" id="inputNewPassword" v-model="state.newPassword" />
                    <div v-if="v$.newPassword.$error" class="form-text text-danger rtl input-validation-text">
                        حداقل طول برای کلمه عبور
                          {{ password_minLength }}
                          است.
                    </div>

                </div>
            </div>
            <div class="row mt-2">
                <div class="col">
                    <label for="inputConfirmPassword"> تکرار کلمه عبور جدید</label>
                    <input type="password" class="form-control ltr" id="inputConfirmPassword" v-model="state.confirmPassword" />
                   
                    <div
                        v-if="v$.confirmPassword.$error"
                        class="form-text text-danger rtl input-validation-text"
                    >
                        <span v-if="v$.confirmPassword.$errors[0].$validator === 'required'">
                         تکرار کلمه عبور را وارد نمایید.
                        </span>
                        <span
                        v-if="v$.confirmPassword.$errors[0].$validator === 'requiredIf'"
                        >
                         کلمه عبور جدید و تکرار آن باید یکسان باشد.
                        </span>
                    </div>
                </div>
            </div>
            <div class="row" style="text-align: end;">
                <div class="col">
                    <button type="submit" class="btn btn-primary mt-4" :disabled="formIsLoading">
                        <div v-if="formIsLoading">
                            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true">

                            </span>
                            صبر کنید ...
                        </div>
                        <span v-else> ثبت </span>

                    </button>
                </div>
            </div>
        </form>
    </div>
</template>

<script>
import { reactive, computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useVuelidate } from '@vuelidate/core';
import { required, minLength } from '@vuelidate/validators';
import { useStore } from 'vuex';
import { sendRequest, requestErrorHandling, showToast } from './../../utility';

export default {
    name: 'ChangeMyPasswordForm',
    setup() {

        const router = useRouter();
        const store = useStore();

        let formIsLoading = ref(false);
        let messages = ref([]);

        const password_minLength = 6;

        const state = reactive({
            currentPassword: null,
            newPassword: null,
            confirmPassword: null,
        });

        const rules = computed(() => ({
            currentPassword: {
                required
            },
            newPassword: {
                required,
                minLength: minLength(password_minLength),
            },
            confirmPassword: {
                required,
                 requiredIf: () =>
                     state.newPassword === state.confirmPassword,
            }
        }));

        const v$ = useVuelidate(rules, state);

        async function submit() {
            messages.value = [];

            const formIsValid = await v$.value.$validate();

            if (formIsValid) {

                const params = {
                    currentPassword: state.currentPassword,
                    newPassword: state.newPassword,
                    confirmPassword: state.confirmPassword
                };

                formIsLoading.value = true;

                try {

                    await sendRequest('PATCH', '/api/changepassword', params, store.getters.getAuthInfo?.userToken);

                    showToast('success', 'تغییر کلمه عبور شما با موفقیت انجام شد.');

                    store.commit('removeAuthInfo');
                    router.replace({ name: 'login' });

                } catch (error) {
                    messages.value = requestErrorHandling(error, router);
                } finally {
                    formIsLoading.value = false;
                }
            }
        }

        function clearMessages() {
            messages.value = [];
        }

        return {
            state,
            password_minLength,
            v$,
            submit,
            formIsLoading,
            messages,
            clearMessages
        };

    }
}
</script>

<style scoped>
#form-container {
    width: 90%;
    margin: 0px auto;
}

@media screen and (min-width:768px) {
    #form-container {
        max-width: 50%;
    }
}
</style>