var optComp1 = "<option class='complement' value='cisco-ios'>Cisco IOS/IOS XE</option>";
var optComp2 = "<option class='complement' value='cisco-asa'>Cisco ASA</option>";
var optNComp1 = "<option class='non-complement' value='cisco-ios-route-list'>Cisco IOS/IOS XE Route List</option>";
var optNComp2 = "<option class='non-complement' value='cisco-ios-acl-list'>Cisco IOS/IOS XE ACL List</option>";
var optBoth1 = "<option class='both' value='list-subnet-masks'>List of subnet/mask</option>";
var optBoth2 = "<option class='both' value='list-cidr'>List of CIDR</option>";
var optBoth3 = "<option class='both' value='csv-subnet-masks'>CSV list of subnet/mask</option>";
var optBoth4 = "<option class='both' value='csv-cidr'>CSV list of CIDR</option>";

function initLoad() {
    $('#non-complement-wm').trigger("click");
}

$(document).ready(function () {
    //To ensure parameters sent to the controler don't include %20%25
    $.ajaxSettings.traditional = true;

    window.setTimeout(initLoad, 10);
    $('#complement-wm').trigger("click");
    $('#outputformat').show();

    // Hide relevant menu options when complement mode selected
    $('#complement-wm').click(function () {
        calculateComp = true;
        $('#outputformat').children().remove();
        $('#outputformat').append(optComp1, optComp2, optBoth1, optBoth2, optBoth3, optBoth4);
    });

    // Hide relevant menu options when non complement mode selected
    $('#non-complement-wm').click(function () {
        calculateComp = false;
        $('#outputformat').children().remove();
        $('#outputformat').append(optNComp1, optNComp2, optBoth1, optBoth2, optBoth3, optBoth4);
    });

    //Check all regions if clicked
    $('#checkAllregion').click(function () {
        $("input[name=region]").prop('checked', true);
    });

    //Uncheck all regions if clicked 
    $('#uncheckAllregion').click(function () {
        $("input[name=region]").prop('checked', false);
    });

    //Check all o365 services if clicked
    $('#checkAllO365').click(function () {
        $("input[name=o365service]").prop('checked', true);
    });

    //Uncheck all o365 services if clicked 
    $('#uncheckAllO365').click(function () {
        $("input[name=o365service]").prop('checked', false);
    });

    //Hide the box if button is clicked
    $('#hideContentButton').click(function () {
        $('#TextResponse').hide(500);
        $('#tbox').html("");
        $(this).hide();
    });

    //Show content if button is clicked.
    $('#showContentButton').click(function () {
        var Controller = 'api/Generate';
        var Command = 'show';
        var Outputformat = $('#outputformat').find('option:selected').val();
        var Region = $('input[name=region]:checked').map(function () { return this.value; }).get();
        var O365svc = $('input[name=o365service]:checked').map(function () { return this.value; }).get();
        // if at least 1 region is selected
        if (Region.length > 0 || O365svc.length > 0) {
            $('#tbox').html("Loading...");
            switch (calculateComp) {
                case true:
                    $('#IPRangeStats').text("Number of complement prefixes found ... : ");
                    break;
                case false:
                    $('#IPRangeStats').text("Number of prefixes found ... : ");
                    break;
            }
            $('#TextResponse').show(500);
            // show button to hide content
            $('#hideContentButton').show();
            $.get(Controller, { command: Command, outputformat: Outputformat, region: Region, o365service: O365svc, complement: calculateComp }, function (responseTxt, statusTxt, xhr) {
                if (statusTxt === "success") {
                    $('#IPRangeStats').append(xhr.responseJSON["count"]);
                    $("#tbox").html(xhr.responseJSON["encodedResultString"]);
                }
                if (statusTxt === "Error")
                    $("#tbox").html("Error fetching the data!" + xhr.statusTxt);
            });
        }
        else
            alert("No Region or Office 365 Service Selected.");
    });

    //DownloadButton - code to be removed
    //$('#downloadContentButton').click(function () {
    //    var Controller = 'download';
    //    var Command = 'download';
    //    var Outputformat = $('#outputformat').find('option:selected').val();
    //    var Region = $('input[name=regionchk]:checked').map(function () { return this.value }).get();
    //    var O365svc = $('input[name=o365servicechk]:checked').map(function () { return this.value }).get();
    //    // Launch controller for download (command)
    //    if (Region.length > 0 || O365svc.length > 0) {
    //        $.get(Controller, { command: Command, outputformat: Outputformat, regions: Region, o365services: O365svc, complement: calculateComp }, function (responseTxt, statusTxt, xhr) {
    //            if (statusTxt === "success") {
    //                // Prepare file for download
    //                var filedata = [];
    //                filedata.push(xhr.responseJSON["encodedResultString"]);
    //                var properties = { type: 'plain/text' };
    //                blobfile = new Blob(filedata, properties);
    //                url = URL.createObjectURL(blobfile);
    //                // Offer the option to open or save file
    //                window.navigator.msSaveOrOpenBlob(blobfile, xhr.responseJSON["fileName"]);
    //            }
    //            if (statusTxt === "Error")
    //                Alert("Error fetching the data!" + xhr.statusTxt);
    //        });
    //    }
    //    else {
    //        alert("No Region or Office 365 Service Selected.");
    //    }
    //});
});