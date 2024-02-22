function displayMessage(componentName, message, color) {
    if (componentName !== '') {
        document.getElementById(componentName).style.color = color;
        $('#' + componentName).text(message);
    }
}

function RunAction(typeName, url, dataArr) {
    return $.ajax({
        url: url,
        type: typeName,
        data: dataArr,
        contentType: "application/json; charset=utf-8",
        async: !1,
        dataType: "json",
        failure: function (response) {
            alert(response.responseText);
        },
        error: function (response) {
            alert(response.responseText);
        }
    });
}

function CopySelectedOne(id) {
    var elementTocopyText = document.getElementById(id);
    elementTocopyText.focus();
    var elementIsVisible = (elementTocopyText.style.visibility === 'hidden') ? true : false;
    if (elementTocopyText.style.visibility === 'hidden')
        elementTocopyText.style.visibility = 'visible';
    elementTocopyText.select();
    elementTocopyText.setSelectionRange(0, 99999);
    document.execCommand("copy");

    if (elementTocopyText.style.visibility === 'visible' && elementIsVisible === true)
        elementTocopyText.style.visibility = 'hidden';
}

function InitQuery(QueryText,btnElement,lblElement) {
    if (QueryText == null || QueryText == "") {
        document.getElementById(btnElement).style.visibility = 'hidden';
        document.getElementById(lblElement).value = "";
    }
    else {
        document.getElementById(btnElement).style.visibility = 'visible';
        document.getElementById(lblElement).value = QueryText;
    }
}

function ProcessResponseMsg(ajaxReturnValue, lblName_msgDisplay) {
    if (ajaxReturnValue === null && ajaxReturnValue.responseJSON === null) {
        displayMessage(lblName_msgDisplay, "No Data Found", "Red");
        return false;
    }
    if (ajaxReturnValue.responseJSON.errorMsg !== null &&
        ajaxReturnValue.responseJSON.errorMsg !== '' &&
        typeof ajaxReturnValue.responseJSON.errorMsg !== 'undefined') {
        displayMessage(lblName_msgDisplay, ajaxReturnValue.responseJSON.errorMsg, "Red");
        return false;

    }

    if (ajaxReturnValue.responseJSON.data === '' ||
        typeof ajaxReturnValue.responseJSON.data === "undefined") {
        displayMessage(lblName_msgDisplay, "No Data Found", "Red");
        return false;
    }

    var arrLen = ajaxReturnValue.responseJSON.data.length;
    displayMessage(lblName_msgDisplay, arrLen + ' records found', 'Black');
    return true;
}

function OpenOtherWindow(urlToOpenInNewWindow,data) {

    let newWind = window.open(urlToOpenInNewWindow);
    var readystatecheckinterval = setInterval(function () {
        if (newWind.document.readyState === "complete" ||
            newWind.document.readyState === "loaded") {
            clearInterval(readystatecheckinterval);
            newWind.ExeQuery(data);
        }
    }, 300);
}

function GetAPIData(urlName, data, lblName) {
    //debugger;
    var urlPath = GetURLPath(urlName);

    var ajaxReturnValue = RunAction("GET", urlPath, data);

    var isDataNull = ProcessResponseMsg(ajaxReturnValue, lblName);

    if (isDataNull === false)
        return '';

    return ajaxReturnValue.responseJSON;
}

function GetURLPath(urlName) {
    var urlPath;
    switch (urlName) {
        case 'GetiPhoneTransactionLogs':
            urlPath = "/UAMobileTool/iPhoneTransaction/GetiPhoneTransactionLogs";
            break;
        case 'GetActionErrors':
            urlPath = "/UAMobileTool/ProdAlerts/GetActionErrors";
            break;            
        case 'GetMinsLogs':
            urlPath = "/UAMobileTool/ExceptionCountInMins/GetMinsLogs";
            break;
        case 'GetAction':
            urlPath = "/UAMobileTool/Testing/GetAction";
            break;
    }
    return urlPath;
}

function DataTableRowValueSetClick(MethodTocall, dataToDisplay) {

    return '<div style="padding:0px;word-wrap: break-word;"><label class="btn-link lnk" style="padding:0px;word-wrap: break-word;margin:0px:width:100%" onclick="' + MethodTocall + '">' + dataToDisplay + '</label></div>';
}
function DataTableRowValueSetNoClick( dataToDisplay) {

    return '<div style="padding:0px;word-wrap: break-word;"><label class="btn-link lnk" style="padding:0px;word-wrap: break-word;margin:0px:width:100%" >' + dataToDisplay + '</label></div>';
}