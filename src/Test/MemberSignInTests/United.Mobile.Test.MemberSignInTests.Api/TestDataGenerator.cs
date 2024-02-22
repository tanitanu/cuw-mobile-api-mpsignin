using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using United.Mobile.Model.Common;
using United.Mobile.Model.MPSignIn.MPNumberToPNR;
using United.Services.Customer.Common;

namespace United.Mobile.Test.MemberSignInTests.Api
{
   public class TestDataGenerator
    {
        private static string GetFileContent(string fileName)
        {
            var path = Path.IsPathRooted(fileName) ? fileName
                                                : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }
        public static IEnumerable<object[]> InputMemeberSignIn()
        {
            var filename = GetFileContent("MOBMPEnRollmentRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBMPEnRollmentRequest>>(filename);
            var resp = GetFileContent("EnrollResponse.json");
            var response = JsonConvert.DeserializeObject<List<EnrollResponse>>(resp);

            yield return new object[] { List[0], response[0] };
            yield return new object[] { List[1], response[0] };
            yield return new object[] { List[1], response[2] };
            yield return new object[] { List[1], response[3] };
            yield return new object[] { List[1], response[1] };
            yield return new object[] { List[2], response[1] };
        }
        public static IEnumerable<object[]> InputMemeberSignIn2()
        {
            var filename = GetFileContent("MOBJoinMileagePlusEnrollmentRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBJoinMileagePlusEnrollmentRequest>>(filename);

            yield return new object[] { List[0] };
            yield return new object[] { List[1] };
        }

        public static IEnumerable<object[]> InputMemeberSignIn2_exc()
        {
            var filename = GetFileContent("MOBJoinMileagePlusEnrollmentRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBJoinMileagePlusEnrollmentRequest>>(filename);

            yield return new object[] { List[1] };
        }
        public static IEnumerable<object[]> InputMemeberSignIn3()
        {
            var filename = GetFileContent("MOBMPSignInNeedHelpRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBMPSignInNeedHelpRequest>>(filename);
            var data = JsonConvert.DeserializeObject<List<MOBMPAccountValidation>>(GetFileContent("MOBMPAccountValidation.json"));

            yield return new object[] { List[0], data[0], false };
            yield return new object[] { List[1], data[1], false };
            yield return new object[] { List[1], data[1], true };
            yield return new object[] { List[1], data[0], false };
            yield return new object[] { List[2], data[0], true };
            yield return new object[] { List[2], data[0], false };
            yield return new object[] { List[3], data[0], false };
            yield return new object[] { List[3], data[0], true };
            yield return new object[] { List[4], data[0], true };
            yield return new object[] { List[4], data[0], false };
            yield return new object[] { List[5], data[0], false };
            yield return new object[] { List[6], data[0], true };
            yield return new object[] { List[6], data[0], false };
            yield return new object[] { List[7], data[0], false };
            yield return new object[] { List[8], data[0], false };
            yield return new object[] { List[9], data[0], false };
            yield return new object[] { List[10], data[0], false };
        }

        public static IEnumerable<object[]> InputMemeberSignIn3_neg()
        {
            var filename = GetFileContent("MOBMPSignInNeedHelpRequest1.json");
            var List = JsonConvert.DeserializeObject<List<MOBMPSignInNeedHelpRequest>>(filename);
            var data = JsonConvert.DeserializeObject<List<MOBMPAccountValidation>>(GetFileContent("MOBMPAccountValidation.json"));

            yield return new object[] { List[0], data[0], false };
            yield return new object[] { List[1], data[1], false };
            
        }
        public static IEnumerable<object[]> InputMemeberSignIn4()
        {
            var filename = GetFileContent("MOBTFAMPDeviceRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBTFAMPDeviceRequest>>(filename);

           yield return new object[] { List[0] };
           yield return new object[] { List[1] };
           yield return new object[] { List[2] };
           yield return new object[] { List[3] };

        }

        public static IEnumerable<object[]> InputAddMpToPNR1()
        {
            var filename = GetFileContent("MPSearchRequest.json");
            var List = JsonConvert.DeserializeObject<List<MPSearchRequest>>(filename);

            yield return new object[] { List[0] };
            yield return new object[] { List[1] };
            yield return new object[] { List[2] };
            yield return new object[] { List[3] };

        }
        public static IEnumerable<object[]> InputAddMpToPNR2()
        {
            var filename = GetFileContent("AddMPNumberToPnrRequest.json");
            var List = JsonConvert.DeserializeObject<List<AddMPNumberToPnrRequest>>(filename);

            yield return new object[] { List[0] };
            yield return new object[] { List[1] };

        }
    }
}
