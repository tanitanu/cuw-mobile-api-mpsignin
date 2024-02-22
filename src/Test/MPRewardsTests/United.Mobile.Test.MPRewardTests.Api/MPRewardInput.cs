using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using United.Mobile.Model.Common;
using United.Mobile.Model.MPRewards;

namespace United.Mobile.Test.MPRewardTests.Api
{
    public class MPRewardInput
    {
        private static string GetFileContent(string fileName)
        {
            var path = Path.IsPathRooted(fileName) ? fileName
                                                : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }
        public static IEnumerable<object[]> InputMPRewards()
        {
            var filename = GetFileContent("GetAccountPlusPointsDetailsRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBMPPlusPointsRequest>>(filename);
            //var file = GetFileContent("MPRewardsResponse.json");
            //var Data = JsonConvert.DeserializeObject<List<MOBMPPlusPointsResponse>>(file);
            yield return new object[] { JsonConvert.SerializeObject(List[0]) };
            yield return new object[] { JsonConvert.SerializeObject(List[1]) };
        }

        public static IEnumerable<object[]> InputMPRewards1()
        {
            var filename = GetFileContent("GetCancelFFCPnrsByMPNumberRequest.json");
            var List = JsonConvert.DeserializeObject<List<MOBCancelFFCPNRsByMPNumberRequest>>(filename);
            //var file = GetFileContent("MPRewardsResponse.json");
            //var Data = JsonConvert.DeserializeObject<List<MOBCancelFFCPNRsByMPNumberResponse>>(file);

            yield return new object[] { JsonConvert.SerializeObject(List[0]), true };
            yield return new object[] { JsonConvert.SerializeObject(List[0]), false };
            yield return new object[] { JsonConvert.SerializeObject(List[1]), true };
        }

        public static IEnumerable<object[]> InputMPRewards11()
        {
            var filename = GetFileContent("GetCancelFFCPnrsByMPNumberRequest1.json");
            var List = JsonConvert.DeserializeObject<List<MOBCancelFFCPNRsByMPNumberRequest>>(filename);
            //var file = GetFileContent("MPRewardsResponse.json");
            //var Data = JsonConvert.DeserializeObject<List<MOBCancelFFCPNRsByMPNumberResponse>>(file);

            yield return new object[] { JsonConvert.SerializeObject(List[0]), true };

        }

        //public static IEnumerable<object[]> InputMPRewards2()
        //{
        //    var filename = GetFileContent("GetActionDetailsForOffersRequest.json");
        //    var List = JsonConvert.DeserializeObject<List<MOBGetActionDetailsForOffersRequest>>(filename);
        //    //var file = GetFileContent("MPRewardsResponse.json");
        //    //var Data = JsonConvert.DeserializeObject<List<MOBCancelFFCPNRsByMPNumberResponse>>(file);

        //    yield return new object[] { JsonConvert.SerializeObject(List[0]) };
        //}
    }
}
