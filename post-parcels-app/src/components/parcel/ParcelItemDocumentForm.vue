<template>
    <div class="alert alert-secondary form-title-container" role="alert">
        <div class="d-flex justify-content-between">
        <div>
            <span>
            مستندات پکیج
            <span v-if="parcelCode" class="ltr"> {{ parcelCode }} </span>
            </span>
        
        </div>
        <div>
             <router-link :to="{ name: 'parcelItem', params: { editId: editId } }"> ویرایش پکیج </router-link>
             &nbsp;
            <router-link :to="{ name: 'parcelList' }"> فهرست پکیج‌ها </router-link>
        </div>
        </div>
    </div>
    <div
      v-show="messages.length > 0"
      class="alert alert-danger alert-dismissible fade show"
      style="margin: 20px 0px;"
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
    <div v-if="imageIsSelected" style="text-align: center;">
        
        <div style="margin-bottom:40px"> 
            <b-icon-filetype-pdf v-if="state.isPDF"  style="font-size: 3em;" />
            <img v-else class="doc" :src="state.preview"  />           
            
        </div>
       <p>
        <button @click="submitFile"  class="btn btn-success btn-sm">ارسال این فایل </button>
        &nbsp; &nbsp;
        <button  @click="cancelSelectedFile"  class="btn btn-danger btn-sm">لغو انتخاب فایل</button>
       </p>
    
        
    </div>
    <div v-else>
        <div v-if="initLoading" style="text-align: center">
            <div class="spinner-border text-secondary" role="status">
            <span class="visually-hidden">Loading...</span>
            </div>
        </div>
        <div v-else style="text-align:center">
            <div style="text-align: right; margin-bottom:50px">
                <input id="fileUploadInput" accept="image/jpeg,image/png,application/pdf" type="file" @change ="onFileChanged($event)" ref="state.image" style="display: none;">
                <button @click="openInputFile" class="btn btn-primary btn-sm">انتخاب فایل جدید</button>
            </div>
            <div v-for="item of docsList" :key="item.id" style="margin-bottom: 90px;">
                <div>
                    <a v-if="item.isPDF" :href="item.path" target="_blank"> <b-icon-filetype-pdf  style="font-size: 4em;" /> </a>  
                    <img v-else class="doc" :src="item.path" />                    
                </div>
                <button @click="setIdForDelete(item.id)" class="btn btn-danger btn-sm" style="margin-top:10px; position:absolute; right:100px;" data-bs-toggle="modal" data-bs-target="#confirmModal"> <b-icon-trash-fill  />  حذف فایل </button>
              
                
            </div>
          
            <div v-if="(!docsList || docsList.length === 0) && initLoading === false">
                <div class="alert alert-info sm" role="alert">
                    داده‌ای برای نمایش یافت نشد.
                </div>
            </div>
        </div>
    </div>
    <div  class="modal fade" id="confirmModal" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="confirmModalLabel" aria-hidden="true">
              <div class=" modal-dialog modal-dialog-centered modal-dialog-scrollable">
                  <div class="modal-content">
                      <div class="modal-header">
                          <h4 class="modal-title fs-5" id="signoutModalLabel"> حذف فایل</h4>
                          <button  type="button"  class="btn-close" data-bs-dismiss="modal" aria-label="Close" @click="closeSignoutModal">
                
                          </button>
                      </div>
                      <div class="modal-body">
                         آیا از حذف این فایل اطمینان دارید؟
                      </div>
                      <div class="modal-footer">
                              <button class="btn btn-danger btn-sm" data-bs-dismiss="modal">
                              خیر
                          </button>
                          &nbsp;
                          <button class="btn btn-success btn-sm"  data-bs-dismiss="modal" @click="deleteImage()">
                              بله
                          </button>
                    
                          </div>
                  </div>
              </div>
      </div>
   
</template>
<script>
    import { onMounted, ref, reactive } from "vue";
    import { useRouter } from "vue-router";
    import { useStore } from "vuex";
    import { sendRequestByFormData, sendRequest, showToast, requestErrorHandling, baseAPI } from "./../../utility";
    export default {
        name: "ParcelItemDocumentForm",
        props: ["editId"],
        setup(props) {

            const router = useRouter();
            const store = useStore();

            const parcelId = ref(props.editId);
            let docsList = ref([]);
            let formIsLoading = ref(false);
            let parcelCode = ref("");
            let selectedIdForDelete = ref(0);
            let initLoading = ref(true);
            let imageIsSelected = ref(false);
            let messages = ref([]);

            const state = reactive({
                image: File,
                preview: File,
                isPDF: Boolean
            });

            onMounted(async () => {
                // Access to template ref and using await/async in root of 'setup()' will be possible in 'onMounted'.

                await getList();

                initLoading.value = false;
            });

            async function getList() {

                try {
                    const result = await sendRequest(
                        "GET",
                        `/api/parcels/${parcelId.value}/documents`,
                        undefined,
                        store.getters.getAuthInfo?.userToken
                    );
                    parcelCode.value = result.parcelCode;
                    if (result.list && result.list.length > 0) {
                        result.list.forEach((element) => {
                            const lastPartOfPath = element.path.split(/[\\/]/).pop();
                            docsList.value.push({
                                id: element.id,
                                path: `${baseAPI}/${element.path}`,
                                isPDF: String(lastPartOfPath.slice(lastPartOfPath.lastIndexOf(".") + 1)) === 'pdf'
                            });
                        });
                    
                    }
                } catch (error) {
                    messages.value = requestErrorHandling(error, router);
                }
            }

            function openInputFile() {
                document.getElementById("fileUploadInput").click()
            }

            function onFileChanged(event) {
                const target = event.target;
                if (target && target.files) {
                    this.state.isPDF = target.files[0].name.split('.')[1].toLowerCase() === 'pdf';
                    this.state.image = target.files[0];
                    this.state.preview = URL.createObjectURL(target.files[0]);
                    this.imageIsSelected = true;
                }
            }

            function cancelSelectedFile() {
                state.image = null;
                state.preview = null;
                imageIsSelected.value = false;
            }
           
            async function submitFile() {
                
                formIsLoading.value = true;

                try {
                    const formData = new FormData();
                    formData.append("image", this.state.image);

                    const response = await sendRequestByFormData(`/api/parcels/${parcelId.value}/documents/add`, formData, store.getters.getAuthInfo?.userToken);
                    
                    docsList.value.push({id: response.id, path: `${baseAPI}/${response.path}`});         
                   
                    showToast("success",`مستند جدید ثبت شد.`);
                    
                    cancelSelectedFile();

                } catch (error) {
                 
                  messages.value = error.response?.data ? [error.response.data.error] : requestErrorHandling(error, router);
                } finally {
                    formIsLoading.value = false;
                }             
                
            }

            function setIdForDelete(id) {
                selectedIdForDelete.value = id;
            }

            async function deleteImage() {
                this.initLoading = true;
                try {
                    await sendRequest(
                        "DELETE",
                        `/api/parcels/documents/${selectedIdForDelete.value}`,
                        undefined,
                        store.getters.getAuthInfo?.userToken
                    );

                    docsList.value.splice(docsList.value.findIndex(item => item.id === selectedIdForDelete.value),1);
                    
                } catch (error) {
                    messages.value = requestErrorHandling(error, router);
                } finally {
                    this.initLoading = false;
                }

                
            }
     
            return {
                openInputFile,
                onFileChanged,
                submitFile,
                formIsLoading,
                initLoading,
                messages,
                state,
                parcelCode,
                docsList,
                cancelSelectedFile,
                imageIsSelected,
                deleteImage,
                setIdForDelete
            }
        }
    }
</script>

<style scoped>
    img.doc {
        max-width: 90%;
        border: 1px solid gray;
        border-radius: 5px;
    }
</style>