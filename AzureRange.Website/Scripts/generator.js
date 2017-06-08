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
    //To ensure parameters sent to the controler are not encoded URLs (%20...)
    $.ajaxSettings.traditional = true;

    window.setTimeout(initLoad, 10);
    $('#complement-wm').trigger("click");
    $('#outputformat').show();

    // Tabulation changes!
    $('.tab').click(function () {
        // If click on one of the service tabs
        // check if the tab is already selected
        if ($(this).attr('class')==="tab-selected")
        {
            //alert('selected tab!');
            // do nothing - just clicked selected tab
        }
        else
        {
            //alert('unselected tab!');
            // deselect other guys
            $(".tab-selected").addClass("tab-nonselected").removeClass("tab-selected");
            $(this).addClass("tab-selected").removeClass("tab-nonselected");
            // make dissapear other tab
            $(".window").hide();
            // Find section name to show
            var sectionname = $(this).attr("id");
            sectionname = sectionname.substring(4, sectionname.length);
            // make appear new tab
            $("#" + sectionname).show();
            $('#tbox').html = "";
            $('#TextResponse').hide();
            // Hide comments for hidden tab
            $(".backgroundText").hide();
            // Show comments for selected tab
            $(".comment-" + sectionname).show();
        }
    });

    $('.tableLabel').click(function () {
        // Toggle regions selected
        if ($(this).attr('chked') !== "true")
        {
            $(this).parent().find("input[type=checkbox]").prop('checked', true);
            $(this).attr('chked', "true");
        }
        else
        {
            $(this).parent().find("input[type=checkbox]").prop('checked', false);
            $(this).attr('chked', "false");
        }
    });

    // Reset parent regional click
    $('input[name=region]').click(function () {
        if ($(this).prop('checked') === false)
        {
            $(this).parent().parent().parent().children("span").attr('chked', "false");
        }
    });

    // Hide relevant menu options when complement mode selected
    $('#complement-wm').click(function () {
        calculateComp = true;
        $('#outputformat').children().remove();
        $('#outputformat').append(optComp1, optComp2, optBoth1, optBoth2, optBoth3, optBoth4);
        $('#summarize-wm').attr('disabled', 'disabled');
        $('#summarize-wm').prop('checked', false);
    });

    // Hide relevant menu options when non complement mode selected
    $('#non-complement-wm').click(function () {
        calculateComp = false;
        $('#outputformat').children().remove();
        $('#outputformat').append(optNComp1, optNComp2, optBoth1, optBoth2, optBoth3, optBoth4);
        $('#summarize-wm').removeAttr('disabled');
        //$('#summarize-wm').show();
    });

    $('.neverTouched').focus(function () {
        // check if box has been checked
        if ($(this).hasClass("neverTouched")) {
            // clear text content
            $(this).val("");
            $(this).removeClass("neverTouched");
        }
    });

    $('#searchedIP').keypress(function (e) {
        if (e.keyCode == 13)
        {
            $('#findPrefix').click();
        }
    });

    //Check all regions if clicked
    $('#checkAllregion').click(function () {
        $("input[name=region]").prop('checked', true);
        $('.tableLabel').attr('chked', "true");
    });

    //Uncheck all regions if clicked 
    $('#uncheckAllregion').click(function () {
        $("input[name=region]").prop('checked', false);
        $('.tableLabel').attr('chked', "false");
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
        var Summarize = $('#summarize-wm').prop('checked');

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
            $('#IPRangeStats').show();
            $('#TextResponse').show(500);
            $('#tbox').height(300);
            // show button to hide content
            $('#hideContentButton').show();
            $.get(Controller, { command: Command, outputformat: Outputformat, region: Region, o365service: O365svc, complement: calculateComp, summarize: Summarize }, function (responseTxt, statusTxt, xhr) {
                if (statusTxt === "success") {
                    $('#IPRangeStats').append(xhr.responseJSON["count"]);
                    $("#tbox").html(xhr.responseJSON["encodedResultString"]);
                }
                if (statusTxt === "Error")
                    $("#tbox").html("Error fetching the data! " + xhr.statusTxt);
            });
        }
        else
            alert("No Region or Office 365 Service Selected.");
    });

    $('#findPrefix').click(function () {
        var Controller = 'api/FindPrefix';
        var InputIP = $("#searchedIP").val();
        $('#TextResponse').show(500);
        $('#IPRangeStats').hide();
        $("#tbox").height(100);
        $('#tbox').show();
        $('#tbox').html("Fetching...");
        $.get(Controller, { inputIP: InputIP }, function (responseTxt, statusTxt, xhr) {
            if (statusTxt === "success") {
                $('#tbox').html(xhr.responseJSON[0]);
            }
            if (statusTxt === "Error") {
                $('#tbox').html("Error fetching the data! " + hhr.statusTxt);
            }
        });

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