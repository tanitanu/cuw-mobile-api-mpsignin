﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IHomePageContentService
    {
        Task<string> GetHomePageContents(string token, string requestData, string sessionId);
    }
}
