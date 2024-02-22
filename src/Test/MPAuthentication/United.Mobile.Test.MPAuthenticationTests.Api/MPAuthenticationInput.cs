using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using United.Mobile.Model.Common;

namespace United.Mobile.Test.MPAuthenticationTests.Api
{
   public class MPAuthenticationInput
    {
        private static string GetFileContent(string fileName)
        {
            var path = Path.IsPathRooted(fileName) ? fileName
                                                : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }
        public static IEnumerable<object[]> SecurityQuestionsV2Input()
        {
            var filename = GetFileContent("ValidateTFASecurityQuestionsV2Request.json");
            var List = JsonConvert.DeserializeObject<List<MOBTFAMPDeviceRequest>>(filename);
            //var List1 = JsonConvert.DeserializeObject<List<MOBTFAMPDeviceResponse>>(GetFileContent("ValidateTFASecurityQuestionsV2Response.json"));

            yield return new object[] { JsonConvert.SerializeObject(List[0]), true };
            yield return new object[] { JsonConvert.SerializeObject(List[0]), false };//
            yield return new object[] { JsonConvert.SerializeObject(List[0]), false };//
            yield return new object[] { JsonConvert.SerializeObject(List[1]), true };//exception
            yield return new object[] { JsonConvert.SerializeObject(List[2]), true };//exception
        }

        public static IEnumerable<object[]> SecurityQuestionsV2CorporateInput()
        {
            var filename = GetFileContent("ValidateTFASecurityQuestionsV2Request.json");
            var List = JsonConvert.DeserializeObject<List<MOBTFAMPDeviceRequest>>(filename);


            yield return new object[] { JsonConvert.SerializeObject(List[4]), true };

        }

        public static IEnumerable<object[]> SecurityQuestionsV2EmpInput()
        {
            var filename = GetFileContent("ValidateTFASecurityQuestionsV2Request.json");
            var List = JsonConvert.DeserializeObject<List<MOBTFAMPDeviceRequest>>(filename);


            yield return new object[] { JsonConvert.SerializeObject(List[5]), true };

        }


        public static IEnumerable<object[]> SecurityQuestionsV2ExceptionInput()
        {
            var filename = GetFileContent("ValidateTFASecurityQuestionsV2Request.json");
            var List = JsonConvert.DeserializeObject<List<MOBTFAMPDeviceRequest>>(filename);


            yield return new object[] { JsonConvert.SerializeObject(List[3]) ,true };
            
        }


        public static IEnumerable<object[]> ValidateMPSignInV2Input()
        {
            var filename = GetFileContent("MOBMPPINPWDValidateRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateRequest>>(filename);
            var List1 = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateResponse>>(GetFileContent("MOBMPPINPWDValidateResponse.json"));
            var jsonResponse = JsonConvert.DeserializeObject<List<United.Services.Customer.Common.ProfileResponse>>(GetFileContent("ProfileResponse.json"));
            var DPAccessTokenResponse = JsonConvert.DeserializeObject<List<DPAccessTokenResponse>>(GetFileContent("DPAccessTokenResponse.json"));

            yield return new object[] { List[9], true, List1[8], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[4], true, List1[8], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[6], true, List1[0], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[0], true, List1[0], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[1], true, List1[0], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[1], true, List1[5], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[0], true, List1[5], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[2], true };
            yield return new object[] { List[8], true, List1[12], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[9], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[9], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[1], true };

            yield return new object[] { List[7], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[10], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[2], true };
            yield return new object[] { List[5], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[2], false };
            yield return new object[] { List[5], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[3], false };
            yield return new object[] { List[1], true, List1[6], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[1], true };
            yield return new object[] { List[0], true, List1[0], JsonConvert.SerializeObject(jsonResponse[2]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[3], true, List1[0], JsonConvert.SerializeObject(jsonResponse[3]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[2], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[4], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };

            yield return new object[] { List[2], true, List1[0], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[0], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[3], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };


        }
        public static IEnumerable<object[]> ValidateMPSignInV2EmpInput()
        {
            var filename = GetFileContent("MOBMPPINPWDValidateRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateRequest>>(filename);
            var List1 = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateResponse>>(GetFileContent("MOBMPPINPWDValidateResponse.json"));
            var jsonResponse = JsonConvert.DeserializeObject<List<United.Services.Customer.Common.ProfileResponse>>(GetFileContent("ProfileResponse.json"));
            var DPAccessTokenResponse = JsonConvert.DeserializeObject<List<DPAccessTokenResponse>>(GetFileContent("DPAccessTokenResponse.json"));

            yield return new object[] { List[11], true, List1[13], DPAccessTokenResponse[0], true };
            
        }

        public static IEnumerable<object[]> ValidateMPSignInV2CorporateInput()
        {
            var filename = GetFileContent("MOBMPPINPWDValidateRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateRequest>>(filename);
            var List1 = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateResponse>>(GetFileContent("MOBMPPINPWDValidateResponse.json"));
            var jsonResponse = JsonConvert.DeserializeObject<List<United.Services.Customer.Common.ProfileResponse>>(GetFileContent("ProfileResponse.json"));
            var DPAccessTokenResponse = JsonConvert.DeserializeObject<List<DPAccessTokenResponse>>(GetFileContent("DPAccessTokenResponse.json"));

            yield return new object[] { List[12], true, List1[14],  DPAccessTokenResponse[0], true };



        }

        public static IEnumerable<object[]> ValidateMPSignInV2TouchIdInput()
        {
            var filename = GetFileContent("MOBMPPINPWDValidateRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateRequest>>(filename);
            var List1 = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateResponse>>(GetFileContent("MOBMPPINPWDValidateResponse.json"));
            var jsonResponse = JsonConvert.DeserializeObject<List<United.Services.Customer.Common.ProfileResponse>>(GetFileContent("ProfileResponse.json"));
            var DPAccessTokenResponse = JsonConvert.DeserializeObject<List<DPAccessTokenResponse>>(GetFileContent("DPAccessTokenResponse.json"));

            yield return new object[] { List[13], true, List1[15], DPAccessTokenResponse[0], true };

        }

        public static IEnumerable<object[]> ValidateMPSignInV2ExcInput()
        {
            var filename = GetFileContent("MOBMPPINPWDValidateRequest1.json");
            var List = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateRequest>>(filename);
            var List1 = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateResponse>>(GetFileContent("MOBMPPINPWDValidateResponse1.json"));
            var jsonResponse = JsonConvert.DeserializeObject<List<United.Services.Customer.Common.ProfileResponse>>(GetFileContent("ProfileResponse1.json"));
            var DPAccessTokenResponse = JsonConvert.DeserializeObject<List<DPAccessTokenResponse>>(GetFileContent("DPAccessTokenResponse1.json"));

            yield return new object[] { List[9], true, List1[8], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[4], true, List1[8], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[6], true, List1[0], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[0], true, List1[0], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[1], true, List1[0], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[1], true, List1[5], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[0], true, List1[5], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[2], true };
            yield return new object[] { List[8], true, List1[12], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[9], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[9], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[1], true };
            yield return new object[] { List[7], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[10], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[2], true };
            yield return new object[] { List[5], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[2], false };
            yield return new object[] { List[5], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[3], false };
            yield return new object[] { List[1], true, List1[6], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[1], true };
            yield return new object[] { List[0], true, List1[0], JsonConvert.SerializeObject(jsonResponse[2]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[3], true, List1[0], JsonConvert.SerializeObject(jsonResponse[3]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[2], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[4], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[0], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[0], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[3], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };

        }

        public static IEnumerable<object[]> ValidateMPSignInV2ExceptionInput()
        {
            var filename = GetFileContent("MOBMPPINPWDValidateRequest2.json");
            var List = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateRequest>>(filename);
            var List1 = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateResponse>>(GetFileContent("MOBMPPINPWDValidateResponse2.json"));
            var jsonResponse = JsonConvert.DeserializeObject<List<United.Services.Customer.Common.ProfileResponse>>(GetFileContent("ProfileResponse1.json"));
            var DPAccessTokenResponse = JsonConvert.DeserializeObject<List<DPAccessTokenResponse>>(GetFileContent("DPAccessTokenResponse.json"));

            yield return new object[] { List[9], true, List1[8], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[4], true, List1[8], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[6], true, List1[0], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[0], true, List1[0], JsonConvert.SerializeObject(jsonResponse[0]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[1], true, List1[0], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[1], true, List1[5], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[0], true, List1[5], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[2], true };
            yield return new object[] { List[8], true, List1[12], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[9], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[9], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[1], true };
            yield return new object[] { List[7], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[10], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[5], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[2], true };
            yield return new object[] { List[5], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[2], false };
            yield return new object[] { List[5], true, List1[11], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[3], false };
            yield return new object[] { List[1], true, List1[6], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[1], true };
            yield return new object[] { List[0], true, List1[0], JsonConvert.SerializeObject(jsonResponse[2]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[3], true, List1[0], JsonConvert.SerializeObject(jsonResponse[3]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[2], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[4], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[0], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[0], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            yield return new object[] { List[2], true, List1[3], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };

        }

        public static IEnumerable<object[]> ValidateMPSignInV2Input_neg()
        {
            var filename = GetFileContent("MOBMPPINPWDValidateRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateRequest>>(filename);
            var List1 = JsonConvert.DeserializeObject<List<MOBMPPINPWDValidateResponse>>(GetFileContent("MOBMPPINPWDValidateResponse.json"));
            var jsonResponse = JsonConvert.DeserializeObject<List<United.Services.Customer.Common.ProfileResponse>>(GetFileContent("ProfileResponse2.json"));
            var DPAccessTokenResponse = JsonConvert.DeserializeObject<List<DPAccessTokenResponse>>(GetFileContent("DPAccessTokenResponse2.json"));

            
            yield return new object[] { List[10], true, List1[0], JsonConvert.SerializeObject(jsonResponse[1]), DPAccessTokenResponse[0], true };
            

        }

    }
}
