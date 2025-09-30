<template>
  <div class="alert alert-secondary form-title-container" role="alert">
    <div class="d-flex justify-content-between">
      <div>
        <span v-if="editId">
          ویرایش پکیج
          <span v-if="editParcelCode" class="ltr"> {{ editParcelCode }} </span>
        </span>
        <span v-else> ثبت پکیج جدید </span>
      </div>
      <div>
        <span v-if="editId">  <router-link :to="{ name: 'parcelItemDocuments', params: { editId: editId } }"> مستندات </router-link> &nbsp; </span>
       
        <router-link :to="{ name: 'parcelList' }"> فهرست پکیج‌ها </router-link>
      </div>
    </div>
  </div>
  <div v-if="initLoading" style="text-align: center">
    <div class="spinner-border text-secondary" role="status">
      <span class="visually-hidden">Loading...</span>
    </div>
  </div>
  <div v-else>
    <div
      v-show="messages.length > 0"
      class="alert alert-danger alert-dismissible fade show"
      style="margin-bottom: 50px"
      role="alert"
    >
      <div v-for="mes of messages" :key="mes" class="mt-1">- {{ mes }}</div>
      <button
        type="button"
        class="btn-close"
        aria-label="Close"
        @click="clearMessages"
      ></button>
    </div>
    <form @keydown.enter.prevent="addToPacket" @submit.prevent="submit" >
      <div class="row">
         
        <div class="col-4">
          <select
            class="form-select"
            id="selectPacket"
            v-model="state.selectedPacketNumber"
          >
            <option v-for="item of packets" :key="item.code" :value="item.code">
              {{ item.name }}
            </option>
          </select>
        </div>
        <div class="col">

        </div>
        <div class="col-3" style="text-align: left">
          <button
            @click="addPacket"
            type="button"
            class="form-control btn btn-outline-primary btn"
          >
            <b-icon-plus-circle-fill />
           بسته جدید
          </button>
        </div>
       
       
      </div>
      <div class="row mt-5">
           <div class="col-md-4">
          <label for="selectItemType"> نوع مرسوله * </label>
          <select
            class="form-select"
            id="selectItemType"
            v-model="state.selectedParcelItemTypeCode"
          >
            <option
              v-for="itemType of parcelItemTypes"
              :key="itemType.code"
              :value="itemType.code"
            >
              {{ itemType.name }}
            </option>
          </select>
          <div
            v-if="v$.selectedParcelItemTypeCode.$error"
            class="form-text text-danger rtl input-validation-text"
          >
            نوع مرسوله مشخص نشده است.
          </div>
        </div>
      </div>
      <div class="row mt-4">
         <div class="col">
          <label for="inputItemCode"> شناسه مرسوله * </label>
          <textarea
            rows="3"
            class="form-control ltr"
            id="inputItemCode"
            v-model="state.parcelItemCode"
          > </textarea>
          <div
            v-if="v$.parcelItemCode.$error"
            class="form-text text-danger rtl input-validation-text"
          >
            <span v-if="v$.parcelItemCode.$errors[0].$validator === 'required'">
              شناسه مرسوله مشخص نشده است.
            </span>
            <span
              v-if="v$.parcelItemCode.$errors[0].$validator === 'requiredIf'"
            >
              این شناسه قبلا در این بسته استفاده شده است.
            </span>
          </div>
        </div>
      </div>
      <div class="row mt-5">
        <div class="col">
          <div class="form-check form-check-inline">
            <input class="form-check-input" type="radio" name="senderOption" id="senderSelectionRadio" :value="true" v-model="state.senderSelectionMode" >
            <label class="form-check-label" for="senderSelectionRadio">انتخاب فرستنده</label>
          </div>
          <div class="form-check form-check-inline">
            <input class="form-check-input" type="radio" name="senderOption" id="senderInputRadio" :value="false" v-model="state.senderSelectionMode" >
            <label class="form-check-label" for="senderInputRadio"> ثبت فرستنده </label>
          </div>

        </div>
      </div>
      <div class="row mt-3" v-show="state.senderSelectionMode">
        <div class="col-md-2">
          <label for="inputSenderCode"> کد فرستنده </label>
          <input
            type="text"
            @keydown.enter.prevent="searchUnits('sender')"
            @blur = "searchUnits('sender')"
            class="form-control ltr"
            id="inputSenderCode"
            v-model="state.senderQueryCode"
          />
        </div>
        <div class="col-md-8">
          <label for="inputSenderTitle"> فرستنده مرسوله * </label>
          <div class="form-control" style="background-color: whitesmoke">
            <div
              v-if="state.selectedSender"
              class="d-flex justify-content-start"
            >
              <div class="rtl">{{ state.selectedSender.name }}</div>
              &nbsp;
              <div class="ltr">({{ state.selectedSender.code }})</div>
            </div>
            <div v-else>&nbsp; &nbsp;</div>
          </div>
          <div
            v-if="v$.selectedSender.$error"
            class="form-text text-danger rtl input-validation-text"
          >
            فرستنده مرسوله مشخص نشده است.
          </div>
        </div>
        <div class="col-md-2">
          <label> &nbsp; &nbsp; </label>
          <button
            type="button"
            @click="openModal('انتخاب فرستنده', 'sender')"
            data-bs-toggle="modal"
            data-bs-target="#formModal"
            class="form-control btn btn-outline-primary"
          >
            <b-icon-search />
          </button>
        </div>
      </div>
      <div class="row mt-3" v-show="!state.senderSelectionMode">
         <div class="col">
            <label for="undefinedSender"> ثبت فرستنده </label>
            <input
              type="text"
              class="form-control rtl"
              id="undefinedSender"
              v-model="state.undefinedSender"
            />
            <div
              v-if="v$.undefinedSender.$error"
              class="form-text text-danger rtl input-validation-text"
            >
             فرستنده را وارد نمایید.
            </div>
          </div>
      </div>
        <div class="row mt-5">
          <div class="col">
            <div class="form-check form-check-inline">
              <input class="form-check-input" type="radio" name="receiverOption" id="receiverSelectionRadio" :value="true" v-model="state.receiverSelectionMode" >
              <label class="form-check-label" for="receiverSelectionRadio">انتخاب گیرنده</label>
            </div>
            <div class="form-check form-check-inline">
              <input class="form-check-input" type="radio" name="receiverOption" id="receiverInputRadio" :value="false" v-model="state.receiverSelectionMode" >
              <label class="form-check-label" for="receiverInputRadio"> ثبت گیرنده </label>
            </div>

          </div>
        </div>
      <div class="row mt-3" v-show="state.receiverSelectionMode">
        <div class="col-md-2">
          <label for="inputReceiverCode"> کد گیرنده </label>
          <input
            type="text"
            @keydown.enter.prevent="searchUnits('receiver')"
            @blur = "searchUnits('receiver')"
            class="form-control ltr"
            id="inputReceiverCode"
            v-model="state.receiverQueryCode"
          />
        </div>
        <div class="col-md-8">
          <label for="inputReceiverTitle"> گیرنده مرسوله * </label>
          <div class="form-control" style="background-color: whitesmoke">
            <div
              v-if="state.selectedReceiver"
              class="d-flex justify-content-start"
            >
              <div class="rtl">{{ state.selectedReceiver.name }}</div>
              &nbsp;
              <div class="ltr">({{ state.selectedReceiver.code }})</div>
            </div>
            <div v-else>&nbsp; &nbsp;</div>
          </div>
          <div
            v-if="v$.selectedReceiver.$error"
            class="form-text text-danger rtl input-validation-text"
          >
            گیرنده مرسوله مشخص نشده است.
          </div>
        </div>
        <div class="col-md-2">
          <label> &nbsp; &nbsp; </label>
          <button
            type="button"
            @click="openModal('انتخاب گیرنده', 'receiver')"
            data-bs-toggle="modal"
            data-bs-target="#formModal"
            class="form-control btn btn-outline-primary"
          >
            <b-icon-search />
          </button>
        </div>
      </div>
      <div class="row mt-3" v-show="!state.receiverSelectionMode">
           <div class="col">
              <label for="undefinedReceiver"> ثبت گیرنده </label>
              <input
                type="text"
                class="form-control rtl"
                id="undefinedReceiver"
                v-model="state.undefinedReceiver"
              />
              <div
                v-if="v$.undefinedReceiver.$error"
                class="form-text text-danger rtl input-validation-text"
              >
               گیرنده را وارد نمایید.
              </div>
            </div>
        </div>
      <div class="row mt-5">
        <div class="col">
          <label for="description"> توضیحات مرسوله (اختیاری) </label>
          <textarea
            class="form-control rtl"
            id="description"
            v-model="state.description"
            rows="3"
          >
          </textarea>
        </div>
      </div>
      <div class="row mt-5">
        <div class="col-7"></div>
        <div class="col">
          <button type="button"
            @click="addToPacket"
            class="form-control btn btn-success btn-sm"
          >
            <b-icon-plus-circle-fill />
            ثبت مرسوله در
            {{ selectedPacketTitle }}
          </button>
        </div>
      </div>

      <div v-for="packet of packets" :key="packet.code" class="row mt-5">
        <div class="col">
          <div class="card shadow-sm">
            <div class="card-header d-flex justify-content-between">
              <div>
                <b-icon-envelope style="font-size: 1.4em" />
                &nbsp;
                {{ packet.name }}
              </div>
              <div>
                <b-icon-x-square-fill
                  style="cursor: pointer; color: red"
                  data-bs-toggle="modal"
                  data-bs-target="#confirmationModal"
                  @click="openConfirmModal(packet.code)"
                />
              </div>
            </div>
            <div class="card-body">
              <div
                v-for="(item, index) of parcelItems.filter(
                  (x) => x.packetNumber === packet.code
                )"
                :key="item.code"
              >
                <div class="packet-item d-flex justify-content-between mb-2">
                  <div>
                    <b-icon-card-text />
                    &nbsp;
                    <span style="font-size: 0.9em; font-weight: bold">
                      <span style="border-bottom: 1px solid">
                        {{
                          parcelItemTypes.find((x) => x.code === item.typeCode)
                            .name
                        }}
                      </span>
                      از
                      <span class="text-danger" v-if="item.sender"> {{ item.sender.name }} </span>
                      <span class="text-danger" v-else> {{ item.undefinedSender }} </span>
                      به
                      <span class="text-success" v-if="item.receiver">
                        {{ item.receiver.name }}
                      </span>
                      <span class="text-success" v-else>
                          {{ item.undefinedReceiver }}
                      </span>
                      با شناسه
                      <span
                        class="text-primary"
                        style="display: inline-block; direction: ltr"
                      >
                        {{ item.code }}
                      </span>
                    </span>
                  </div>
                  <div>
                    <span
                      data-bs-toggle="collapse"
                      :href="`#desc-${packet.code}-${index}`"
                      v-if="item.description && item.description.length > 0"
                      class="badge bg-warning text-dark"
                      style="cursor: pointer"
                    >
                      توضیحات
                    </span>
                    &nbsp;
                    <b-icon-trash
                      @click="removeFromPacket(item.code)"
                      style="color: rgb(136, 4, 4); cursor: pointer"
                    />
                  </div>
                </div>
                <div
                  :id="`desc-${packet.code}-${index}`"
                  class="collapse"
                  style="
                    text-align: justify;
                    font-size: 0.8em;
                    margin: 2px 20px;
                    border: 1px solid rgb(221, 221, 221);
                    padding: 5px;
                    border-radius: 5px;
                  "
                >
                  {{ item.description }}
                </div>
              </div>

              <div
                v-if="!parcelItems.find((x) => x.packetNumber === packet.code)"
                style="font-size: 0.8em"
              >
                این بسته خالی است.
              </div>
            </div>
          </div>
        </div>
      </div>
      <div style="text-align: end" class="mt-5">
        <button id="btn-main" class="btn btn-primary mt-4" :disabled="formIsLoading">
          <div v-if="formIsLoading">
            <span
              class="spinner-border spinner-border-sm"
              role="status"
              aria-hidden="true"
            >
            </span>
            صبر کنید ...
          </div>
          <span v-else> ثبت پکیج </span>
        </button>
      </div>
      <div
      v-show="messages.length > 0"
      class="alert alert-danger alert-dismissible fade show"
      style="margin-top: 20px"
      role="alert"
    >
      <div v-for="mes of messages" :key="mes" class="mt-1">- {{ mes }}</div>
      <button
        type="button"
        class="btn-close"
        aria-label="Close"
        @click="clearMessages"
      ></button>
    </div>
    </form>
    <div
      class="modal fade"
      id="formModal"
      data-bs-backdrop="static"
      data-bs-keyboard="false"
      tabindex="-1"
      aria-labelledby="formModalLabel"
      aria-hidden="true"
    >
      <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content">
          <div class="modal-header">
            <h4 class="modal-title fs-5" id="formModalLabel">
              {{ modalTitle }}
            </h4>
            <button
              ref="modalCloseButton"
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
              aria-label="Close"
              @click="closeModal"
            ></button>
          </div>
          <div class="modal-body">
            <div class="row">
              <div class="col">
                <input
                  type="text"
                  @keyup="searchModal"
                  class="form-control form-control-sm rtl"
                  placeholder="کد / عنوان"
                  v-model="modalSearchQuery"
                />
              </div>
            </div>

            <div
              v-if="filteredUnitsList.find((x) => x.parentId === null)"
              class="card card-body mt-3"
              style="
                font-weight: bold;
                font-size: 0.9em;
                max-height: 400px;
                overflow: auto;
              "
            >
              <div
                v-for="unit of filteredUnitsList.filter(
                  (x) => x.parentId === null
                )"
                :key="unit.id"
              >
                <b-icon-plus-square
                  v-if="unit.hasChild"
                  data-bs-toggle="collapse"
                  :href="`#childs${unit.code}`"
                  style="cursor: pointer"
                />
                <b-icon-dash v-else />
                <div
                  class="hover-item"
                  @click="selectUnit(unit, modalMood)"
                  style="
                    display: inline-block;
                    width: 95%;
                    margin-inline-start: 5px;
                    cursor: pointer;
                  "
                >
                  {{ unit.name }}
                  <span style="display: inline-block"> ({{ unit.code }}) </span>
                </div>
                <div
                  class="collapse childrenContainer"
                  :id="`childs${unit.code}`"
                  style="margin-inline-start: 16px"
                >
                  <div
                    @click="selectUnit(subUnit, modalMood)"
                    class="hover-item"
                    v-for="subUnit of filteredUnitsList.filter(
                      (x) => x.parentId === unit.id
                    )"
                    :key="subUnit.id"
                    style="margin-top: 1px; cursor: pointer"
                  >
                    <b-icon-arrow-return-left /> {{ subUnit.name }}
                    <span style="display: inline-block">
                      ({{ subUnit.code }})
                    </span>
                  </div>
                </div>
              </div>
            </div>
            <div
              v-else
              class="alert alert-info"
              role="alert"
              style="margin-top: 50px"
            >
              داده‌ای برای نمایش یافت نشد.
            </div>
          </div>
        </div>
      </div>
    </div>
    <div
      class="modal fade"
      id="confirmationModal"
      data-bs-backdrop="static"
      data-bs-keyboard="false"
      tabindex="-1"
      aria-labelledby="confirmationModalLabel"
      aria-hidden="true"
    >
      <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content">
          <div class="modal-header">
            <h4 class="modal-title fs-5" id="confirmationModalLabel">
              تایید حذف
            </h4>
            <button
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
              aria-label="Close"
              @click="closeModal"
            ></button>
          </div>
          <div class="modal-body">
            آیا از حذف بسته شماره
            {{ confirmModalData.selectedPacket }}
            اطمینان دارید؟
          </div>
          <div class="modal-footer">
            <button class="btn btn-warning btn-sm" data-bs-dismiss="modal">
              لغو
            </button>
            &nbsp;
            <button
              class="btn btn-primary btn-sm"
              data-bs-dismiss="modal"
              @click="deletePacket(confirmModalData.selectedPacket)"
            >
              بله
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import { computed, reactive, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { useStore } from "vuex";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { sendRequest, showToast, requestErrorHandling } from "./../../utility";

export default {
  name: "ParcelItemForm",
  props: ["editId"],
  setup(props) {
    const router = useRouter();
    const store = useStore();

    let formIsLoading = ref(false);
    let initLoading = ref(true);
    let messages = ref([]);
    let modalCloseButton = ref(null);

    const parcelItemTypes = [
      { code: 1, name: "پرونده" },
      { code: 2, name: "نامه" },
      { code: 3, name: "اوراق" },
      { code: 4, name: "زونکن" },
      { code: 5, name: "کارتن" },
      { code: 6, name: "گونی" },
    ];

    const editId = ref(props.editId);
    let editParcelCode = ref(null);
    let unitsList = [];
    let filteredUnitsList = ref([]);
    let packets = ref([{ code: 1, name: "بسته شماره ۱" }]);
    let parcelItems = ref([]);

    let modalTitle = ref(null);
    let modalMood = ref(null);
    let modalSearchQuery = ref(null);
    let modalOpenAllChilds = ref(false);
    let confirmModalData = reactive({ selectedPacket: 1 });

    const state = reactive({
      selectedPacketNumber: 1,
      selectedParcelItemTypeCode: parcelItemTypes[0].code,
      parcelItemCode: null,
      selectedSender: null,
      senderQueryCode: null,
      undefinedSender: null,
      selectedReceiver: null,
      receiverQueryCode: null,
      undefinedReceiver: null,
      description: null,
      senderSelectionMode: true,
      receiverSelectionMode: true,
    });

    const rules = computed(() => ({
      selectedPacketNumber: {
        required,
      },
      selectedParcelItemTypeCode: {
        required,
      },
      selectedSender: {
        required,
        /*requiredIf: () => 
          !state.selectedSender && state.senderSelectionMode === true,*/
      },
      undefinedSender: {
        required,
        /*requiredIf: () =>
          (!state.undefinedSender || state.undefinedSender.length === 0) && state.senderSelectionMode === false,*/
      },
      selectedReceiver: {
        required,
        /*requiredIf: () =>
          (!state.selectedReceiver && !state.undefinedReceiver),*/
      },
      undefinedReceiver: {
        required,
      },
      parcelItemCode: {
        required,
        /*requiredIf: () =>
          !parcelItems.value.find((x) => x.code === state.parcelItemCode),*/
      },
    }));

    const v$ = useVuelidate(rules, state);

    onMounted(async () => {
      // Access to template ref and using await/async in root of 'setup()' will be possible in 'onMounted'.

      await getInitData();

      if (editId.value) {
        await getEditItem(editId.value);
      }

      initLoading.value = false;
     
      window.addEventListener('scroll', () => {
        let distanceFromBottom = document.documentElement.scrollHeight - window.innerHeight - window.scrollY;
       
        if (distanceFromBottom > 80) {
          //document.getElementById("btn-main").style.display = "none";
          document.getElementById("btn-main").style.position = "fixed";
          document.getElementById("btn-main").style.right = "10px";
          document.getElementById("btn-main").style.bottom = "120px";
          //document.getElementById("btn-main").style.display = "block";        
        } else {
          //document.getElementById("btn-main").style.display = "none";
          document.getElementById("btn-main").style.position = "static";
          //document.getElementById("btn-main").style.display = "inline";
        }
      });
    });
    
    async function getInitData() {
      try {
        const result = await sendRequest(
          "GET",
          `/api/units?active=true&all=true&onlyParents=false`,
          undefined,
          store.getters.getAuthInfo?.userToken
        );

        if (result.list && result.list.length > 0) {
          result.list.forEach((element) => {
            unitsList.push({
              id: element.id,
              code: element.code,
              name: element.name,
              parentId: element.parentId,
              hasChild: !!result.list.find(
                (x) => x.parentId === element.id
              ),
            });
          });
         
        }
      } catch (error) {
        messages.value = requestErrorHandling(error, router);
      }
    }

    async function getEditItem() {
      try {
        const result = await sendRequest(
          "GET",
          `/api/parcels/${editId.value}`,
          undefined,
          store.getters.getAuthInfo?.userToken
        );
        editParcelCode.value = result.code;
        packets.value = []; // reset for exist packets in editing item
        
        result.list.forEach((element) => {
          if (!packets.value.find((x) => x.code === element.packetNumber)) {
            packets.value.push({
              code: element.packetNumber,
              name: `بسته شماره ${element.packetNumber}`,
            });
          }

          parcelItems.value.push({
            packetNumber: element.packetNumber,
            typeCode: element.type,
            sender: element.undefinedSenderTitle  === null ? unitsList.find((x) => x.id === element.senderId) : null,
            undefinedSender: element.undefinedSenderTitle,
            receiver: element.undefinedReceiverTitle === null ? unitsList.find((x) => x.id === element.receiverId) : null,
            undefinedReceiver: element.undefinedReceiverTitle,
            code: element.code,
            description: element.description,
          });
        });
        
        state.selectedPacketNumber = packets.value.sort((x) => -x.code)[0].code;
        state.selectedParcelItemTypeCode = parcelItemTypes[0].code;
      } catch (error) {
        messages.value = requestErrorHandling(error, router);
      }
    }

    function searchUnits(type) {
      if (type === "receiver") {
        state.selectedReceiver = unitsList.find(
          (x) => x.code === state.receiverQueryCode
        );
      }

      if (type === "sender") {
        state.selectedSender = unitsList.find(
          (x) => x.code === state.senderQueryCode
        );
      }
    }

    async function addToPacket() {
      
      const formIsValid = ((state.senderSelectionMode === true && (await v$.value.selectedSender.$validate()) === true) 
                           || (state.senderSelectionMode === false && (await v$.value.undefinedSender.$validate()) === true)) &&
                          ((state.receiverSelectionMode === true && (await v$.value.selectedReceiver.$validate()) === true)
                           || (state.receiverSelectionMode === false && (await v$.value.undefinedReceiver.$validate()) === true)) &&
                          (await v$.value.selectedPacketNumber.$validate()) === true &&
                          (await v$.value.selectedParcelItemTypeCode.$validate()) === true &&
                          (await v$.value.parcelItemCode.$validate()) === true;
      if(formIsValid === false) {
        await v$.value.$validate();
      } else {
        clearMessages();
        parcelItems.value.push({
          packetNumber: state.selectedPacketNumber,
          typeCode: state.selectedParcelItemTypeCode,
          sender: state.senderSelectionMode ? state.selectedSender : null,
          undefinedSender: state.senderSelectionMode ? null : state.undefinedSender,
          receiver: state.receiverSelectionMode ? state.selectedReceiver : null,
          undefinedReceiver: state.receiverSelectionMode ?  null : state.undefinedReceiver,
          code: state.parcelItemCode,
          description: state.description,
        });
        if(state.senderSelectionMode) {
          state.undefinedSender = null;
        } else {
          state.selectedSender = null;
        }
        if (state.receiverSelectionMode) {
          state.undefinedReceiver = null;
        } else {
          state.selectedReceiver = null;
        }
        state.parcelItemCode = null;
        state.description = null;
        state.selectedParcelItemTypeCode = parcelItemTypes[0].code
        v$.value.$reset();
      }
    }

    function removeFromPacket(code) {
      parcelItems.value.splice(
        parcelItems.value.findIndex((x) => x.code === code),
        1
      );
    }

    async function submit() {
      
      clearMessages();

      const items = [];

      parcelItems.value.forEach((item) => {
        items.push({
          code: item.code,
          packetNumber: item.packetNumber,
          type: item.typeCode,
          senderId: item.sender ? item.sender.id : null,
          undefinedSenderTitle: item.undefinedSender,
          receiverId: item.receiver ? item.receiver.id : null,
          undefinedReceiverTitle: item.undefinedReceiver,
          description: item.description,
        });
      });

      formIsLoading.value = true;

      const func = editId.value ? update : add;

      await func(items);
    }

    async function add(params) {
      try {
        const result = await sendRequest(
          "POST",
          "/api/parcels/add",
          params,
          store.getters.getAuthInfo?.userToken
        );

        resetForm();
        showToast("success",`پکیج با کد ${result.parcelCode} با ${result.itemsCount} عدد بسته با موفقیت ثبت شد.`);

      } catch (error) {
        messages.value = requestErrorHandling(error, router);
      } finally {
        formIsLoading.value = false;
      }
    }

    async function update(params) {
      try {
        await sendRequest(
          "PATCH",
          `/api/parcels/${editId.value}/update`,
          params,
          store.getters.getAuthInfo?.userToken
        );

        //resetForm();
          showToast(
            "success",
            `ویرایش پکیج با کد ${editParcelCode.value} با موفقیت انجام شد.`
          );
          //router.replace({ name: "parcelList" });

      } catch (error) {
        messages.value = requestErrorHandling(error, router);
      } finally {
        formIsLoading.value = false;
      }
    }

    function openModal(headerTitle, mood) {
      modalTitle.value = headerTitle;
      modalMood.value = mood;
      filteredUnitsList.value = unitsList;
      modalSearchQuery.value = null;
    }

    function selectUnit(unit, modalMood) {
      if (modalMood === "receiver") {
        state.selectedReceiver = unit;
        state.receiverQueryCode = unit.code;
      }

      if (modalMood === "sender") {
        state.selectedSender = unit;
        state.senderQueryCode = unit.code;
      }
      modalCloseButton.value.click();
    }

    function searchModal() {
      filteredUnitsList.value = !modalSearchQuery.value
        ? unitsList
        : unitsList.filter(
            (x) =>
              x.code.includes(modalSearchQuery.value) ||
              x.name.includes(modalSearchQuery.value) ||
              unitsList.find(
                (y) =>
                  y.parentId === x.id &&
                  (y.code.includes(modalSearchQuery.value) ||
                    y.name.includes(modalSearchQuery.value))
              )
          );
      modalToggleAllChildren();
    }

    function modalToggleAllChildren() {
      modalOpenAllChilds.value = !!modalSearchQuery.value;
      let childrenContainers = document.querySelectorAll(".childrenContainer");
      childrenContainers.forEach((x) => {
        if (modalOpenAllChilds.value) {
          x.classList.add("show");
        } else {
          x.classList.remove("show");
        }
      });
    }

    function addPacket() {
     
      const newPacketNumber = packets.value[0].code + 1;
      
      packets.value.unshift({
        code: newPacketNumber,
        name: `بسته شماره ${newPacketNumber}`,
      });
      state.selectedPacketNumber = newPacketNumber;
    }

    function deletePacket(packetNumber) {
      if (packets.value.length <= 1) {
        messages.value.unshift("حداقل یک بسته برای هر پکیج ضروری است.");
        return;
      }
      if (packetNumber < 1) {
        messages.value.unshift("شماره بسته صحیح نمی‌باشد.");
        return;
      }

      if (parcelItems.value.find((x) => x.packetNumber === packetNumber)) {
        messages.value.unshift("پیش از حذف بسته آن را خالی نمایید.");
        return;
      }

      packets.value.splice(
        packets.value.findIndex((x) => x.code === packetNumber),
        1
      );
      state.selectedPacketNumber = packets.value[0].code;
    }

    function clearMessages() {
      messages.value = [];
    }

    function resetForm() {
      clearMessages();

      state.selectedPacketNumber = packets.value[0].code;
      state.selectedParcelItemTypeCode = parcelItemTypes[0].code;
      state.parcelItemCode = null;
      state.selectedSender = null;
      state.senderQueryCode = null;
      state.selectedReceiver = null;
      state.receiverQueryCode = null;
      state.description = null;

      packets.value = [{ code: 1, name: selectedPacketTitle }];
      parcelItems.value = [];
    }

    function openConfirmModal(code) {
      confirmModalData.selectedPacket = code;
    }

    const selectedPacketTitle = computed(() => {
      return `بسته شماره ${state.selectedPacketNumber}`;
    });

    return {
      state,
      formIsLoading,
      v$,
      submit,
      clearMessages,
      messages,
      initLoading,
      parcelItemTypes,
      parcelItems,
      packets,
      filteredUnitsList,
      searchUnits,
      openModal,
      modalTitle,
      modalMood,
      selectUnit,
      modalCloseButton,
      modalSearchQuery,
      searchModal,
      addPacket,
      deletePacket,
      confirmModalData,
      openConfirmModal,
      selectedPacketTitle,
      addToPacket,
      removeFromPacket,
      editParcelCode,
    };
  },
};
</script>

<style scoped>
.hover-item {
  background-color: initial;
  padding: 2px 2px;
}
.hover-item:hover {
  background-color: rgb(186, 208, 250);
}

.packet-item {
  background-color: initial;
  padding: 4px 10px;
  border-radius: 4px;
}
.packet-item:hover {
  background-color: rgb(241, 241, 241);
}

textarea {
  resize:vertical;
}

</style>
