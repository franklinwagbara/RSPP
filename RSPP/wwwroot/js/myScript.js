$(document).ready(function () {

    // error message
    function ErrorMessage(error_id, error_message) {
        $(error_id).fadeIn('fast')
                    .html("<i class=\"fa fa-exclamation-triangle  text-danger \"> " + error_message + " </i>")
                    .delay(9000)
                    .fadeOut('fast');
        return;
    }

    // success message
    function SuccessMessage(success_id, success_message) {
        $(success_id).fadeIn('fast')
                    .html("<i class=\"fa fa-check fa-1x text-success\"> " + success_message + " </i>")
                    .delay(10000)
                    .fadeOut('fast');
        return;
    }


   
    /*
     *  toogle for reporting
    */
    $("#ReportAction").on('click', function (e) {
        e.preventDefault();
        $("#ReportLink").toggle(300);
    });


    /*
     *  toogle for Request
    */
    $("#RequestAction").on('click', function (e) {
        e.preventDefault();
        $("#RequestsLink").toggle(300);
    });



    $("#SettingsAction").on('click', function (e) {
        e.preventDefault();
        $("#SettingsLink").toggle(300);
    });



    // keeping session alive 30 sec
    window.setInterval(function () {
            $.get('/REVIEWER/session');                
    }, 300000);

    /* page reload
    window.setInterval(function () {
        location.reload(false);
    }, (60000 * 3)); */ // 3 min

    // function to preview image before sending   
    function showMyImage(fileInput, fileID, error_id, image_name, img_id, img_path) {
        var files = fileInput.files;
        for (var i = 0; i < files.length; i++) {
            var file = files[i];
            var imageType = /image.*/;
            if (!file.type.match(imageType)) {
                ErrorMessage($(error_id), "This image format is not supported.");
                $(image_name).val('');
                $(img_id).attr("src", img_path);
                return false;
            }
            else if (file.size > (1048576 * 2)) { //2MB
                ErrorMessage($(error_id), "Large image size. Minimum of 2MB");
                $(image_name).val('');
                $(img_id).attr("src", img_path);
                return false;
            }

            

            var img = document.getElementById(fileID);
            img.file = file;

            var reader = new FileReader();
            reader.onload = (function (aImg) {
                return function (e) {
                    aImg.src = e.target.result;
                };

            })(img);
            reader.readAsDataURL(file);
           
        }
    }

    function allownumbers(e, error_id, error_text) {
        if (e.shiftKey || e.ctrlKey || e.altKey) {
            e.preventDefault();
            error_id.fadeIn('fast');
            error_id.html("<i class=\"fa fa-exclamation-circle fa-1x text-danger\"> " + error_text + "</i>");
            $(error_id).delay(5000)
                        .fadeOut('fast');
        }
        else {
            var key = e.keyCode;
            if (!((key === 8) || (key === 46) || (key >= 35 && key <= 40) || (key >= 48 && key <= 57) || (key >= 96 && key <= 105))) {
                e.preventDefault();
                error_id.fadeIn('fast');
                error_id.html("<i class=\"fa fa-exclamation-circle fa-1x text-danger\"> " + error_text + "</i>");

                $(error_id).delay(5000)
                            .fadeOut('fast');
            }
        }
    }

    // image changing function for sme logo
    $("#Image").on('change', function () {
        showMyImage(this, "SMEthumbnil", "#errorImage", "#Image", "#SMEthumbnil", "~/images/personal.png");
    });

    $("#txtPhone").on("keydown", function (event) {
        allownumbers(event, $("#erProduct"), "Only numbers allowed for price.");
    });

    //sign up form 

    $("#SignupForm").on('submit', function (e) {
        if ($("#Image").val() === "") {
            e.preventDefault();
            ErrorMessage("#errorImage", "Please upload your image.");
        }
    });

    $("#btnClear").on('click', function (e) {
        e.preventDefault();
        $("#txtCode").val('');
        $("#txtName").val('');
        $("#txtEmail").val('');
        $("#txtPass").val('');
        $("#txtCPass").val('');
        $("#txtPhone").val('');
        $("#SignupForm")[0].reset();
    });


    // loading user account function
    function progress() {
        var elem = document.getElementById('bar');
        var width = 1;
        var id = setInterval(frame, 100);

        function frame() {   
            if (width >= 100) {
                clearInterval(id);
                $("#loadinInfo").text("Completed. Opening dashboard.");

                if ($.trim($("#txtRole").val()) === "REVIEWER" || $.trim($("#txtRole").val()) === "REVIEWER") {
                    window.location = "/REVIEWER/Dashboard";
                }
                else if ($.trim($("#txtRole").val()) === "Admin") {
                    window.location = "/REVIEWER/Dashboard";
                }
            }
            else {
                width++;
                elem.style.width = width + '%';
            }
        }
    }
   
    // loading user account
    $("#progressDiv").on('click', function () {
        progress();
    });


    // TeleScript saving function
    $("#TeleScriptForm").on('submit', function (e) {
      
        if ($("#State").val() === "Select State" || $("#txtQuestion").val() === "" || $("#txtPlatform").val() === "Select Platform" || $("#txtPlatform").val() === "") {
            ErrorMessage("#TeleInfo", "State, Platform and Complain are required");
            e.preventDefault();
        }
       
    });

    //TeleScript clear button
    $("#btnTeleClear").on('click', function (e) {
        e.preventDefault();
        $("#TeleScriptForm")[0].reset();
    });


    // Hide telescript
    $("#btnHide").on('click', function () {
        $("#DivTeleScript").slideToggle();
    });

    $(document).on({

        ajaxStart: function () {
            $("#LoadingFAQ").text("Loading FAQ...");
        },
        ajaxStop: function () {
            $("#LoadingFAQ").text("");
        }


    });


    // Load more FAQ
    $("#btnLoadMore").on('click', function () {
        var id = $(".div-faq:last").attr("id");
        alert(id);
    });

    // Back to top
    $("#TOP").on('click', function (event) {
        event.preventDefault();
        $('html, body').animate({
            scrollTop: 0
        }, 1200);
    });

    $("#txtSearch").on('keyup', function (e) {
        e.preventDefault()
       // $("#btnSearchClient").click();
    });


    // Search clients complain.
    $("#btnSearchClient").on('click', function (e) {
        e.preventDefault();
        var div = $("#SearchResult");
        div.html('');

        if ($("#txtSearch").val() === "") {
            div.html("");
        }
        else {

            $("#CLoad").fadeIn(300);

            $.getJSON('/REVIEWER/SearchClientComplain', { 'txtSearch': $("#txtSearch").val() }, function (data) {

                var count = "";
                var action = "";
                var toggle = "";

                if (data.length == 0) {
                    div.html("");
                }

                if (data.length <= 1) {
                    count = "Complain";
                }
                else if (data.length > 1) {
                    count = "Complains";
                }

                div.append('<h4 class="text-right"><b>' + data.length + ' ' + count +' by <span style="color:rgb(215, 135, 155)"> "' + $("#txtSearch").val() + '"</span> </b></h4> <hr>');
                
                $.each(data, function (key, val) {

                    div.append('<div> <h5 id="TitleHead"> <b>' + val.title.toUpperCase() + ' </b> </h5> </div> ');
                    div.append('<div id="DivComplain"> <b class="text-warning"> Complain </b> : ' + val.complain + '</div> <br>');
                    div.append('<small> <span id="tag" title="Company\'s name"> <i class="fa fa-university">  </i> ' + val.company + ' </span> &nbsp; <span id="tag"> <i class="fa fa-user"></i> ' + val.customer + '</span>  <span id="tag" style="color:maroon"> <i class="fa fa-calendar"></i> ' + val.date + '</span> <span id="tag"> <i class="fa fa-map-marker"></i> ' + val.state + ' | ' + val.lga + '</span> <span id="tag" title="Company\'s contry"> <i class="fa fa-globe">  </i> ' + val.country + ' </span> &nbsp;<span id="tag" style="color:black">' + val.option + '</span> </span> <span id="tag" class="status' + val.tel_id + '" style="color:steelblue">' + val.status + '</span> </small> </span> <span id="tag" title="Agent"> <i class="fa fa-user-md">  </i> ' + val.user + ' </span> &nbsp; <small><span id="tag" title="Platform"> <i class="fa fa-book">  </i> ' + val.platform + ' </span></small>');

                  
                    action = '<span data-toggle="modal" onclick="get_users(' + val.tel_id + ')" data-target="#userModal"><i class="fa fa-mail-forward" title="Forward complain"> Forward </i></span> &nbsp;&nbsp;';
                    

                    if ($.trim(val.state) !== "Active") {
                        toggle = '<i class="fa fa-dot-circle-o" id="toggle' + val.tel_id + '" onclick="toggleStatus('+val.tel_id+')"> Set Active </i>';
                    }
                    else {
                        toggle = '';
                    }
                        
                    div.append('<br><br><div class="pull-right" id="NavOptions">' + action + '<span title="set as active">' + toggle + ' </span> &nbsp; <span id="See'+val.tel_id+'"  onclick="GetAnswers('+ val.tel_id +')" style="cursor:pointer"> <i class="fa fa-comments-o"></i><b> Answers</b> </span> </div> <br>');

                    div.append('<div> <span id="Load'+val.tel_id +'" style="display:none"> <img src="/images/load.gif" style="height:20px; width:20px;"/> getting answers, please wait... </span> <br> <div id="AnsersResults'+val.tel_id+'" style="display:none"> </div> </div>');

                    div.append('')
                    div.append('</div><br><hr><br>');
                       
                });

                $("#CLoad").fadeOut(300);
            });
        }
    });


    // display generated codes for a user and get forwarded requests.
    $("#RequestCount").ready(function () {
        getCodes();
        getRequests();
    });


    // show list of users when document is ready
    $("btnTxtSearch").ready(function () {
        var html = '';

        $("#userSelect").html("");

        $.getJSON('/Admin/SelectUsers', {}, function (datas) {
            $("#userSelect").append("<option></option>");
            $.each(datas, function (key, val) {
                html += '<option>' + val.fullname + '|' + val.user_id + '</option>';
            });
            $("#userSelect").append(html);
        });
    });


    $("#DivAC").ready(function () {
        getRequests();
    });

    // get codes
    function getCodes() {

        $.getJSON('/Admin/GetCode', {}, function (data) {
            var div = $("#GenDiv");
            div.html('');

            $.each(data, function (key, val) {
                div.append('<div style="background:#05161f; color:white; padding:5px"> <small><b>' + val.role + ' == ' + val.code + '</b></small> <i class="fa fa-trash pull-right" id="CodeDelete" onclick="DeleteCode(' + val.user_id + ')"> </i> </div>  <br>');
            });
        });
    }


    $("#RequestCount").text("Loading request...");
    // function to get requests
    function getRequests() {
        $.getJSON('/Admin/getRequest', {}, function (data) {
            $("#RequestCount").text(data);
        });
    }
     
    
    // generate code
    $("#btnGenerate").on('click', function (e) {
        e.preventDefault();

        var url = "/REVIEWER/Dashboard";

        if ($("#Role2").val() === "" || $("#Pass2").val() === "") {
            return;
        }
        else {
            $.post('/Admin/GenerateCode', { 'role': $("#Role2").val(), 'password': $("#Pass2").val() }, function (data) {
                if ($.trim(data) === "inserted") {

                    getCodes();
                    $("#Role2").val('');
                    $("#Pass2").val('');

                    alert("Success\n\n Reference code has been generated for the selected user.");
                }
                else {
                    alert("Error\n\n" + data);
                }
            });
        }
    });
        

    // Load more button for forwarded complains
    $("#f_load_more").on('click', function (e) {
        e.preventDefault();

        var id = $(".forwardDiv:last").attr("id");
        var div = $("#DivLoad");
        var html = '';
        var btn = $("#f_load_more");
        var count = $("#txtCount");

        btn.text('Loading...');

        $.getJSON('/Admin/LoadMoreForwards', { 'for_id': id }, function (data) {

            if (data.length == 0) {
                btn.text('No more request');
            }
            else {

                var total = parseInt(count.text())+parseInt(data.length);
                count.text('');
                count.text(total);

                $.each(data, function (key, val) {

                    html += '<div class="text-left forwardDiv" id="' + val.for_id + '" onmouseover="LoadAnswerCount('+val.tel_id+')">';
                    html += '<h5 id="TitleHead"> <b> ' + val.title + ' </b> | <small id="tag"> Forward Date - <b class="text-danger"><i class="fa fa-calendar-o"> </i> ' + val.date_f + '</b> </small></h5>';
                    html += '<div id="DivComplain"> <b class="text-warning"> Complain : </b> ' + val.complain + ' </div>';
                    html += '<br /> <a href="/Admin/Complain/'+val.tel_id+'/'+val.title+'"> Click to see more .... </a><span id="Count_'+val.for_id+'"> </span>';
                   
                    html += ' <span> &nbsp;&nbsp;&nbsp; <i class="fa fa-envelope-open-o text-warning" onclick="Mark(' + val.for_id + ')"> Mark as resolved</i> &nbsp;&nbsp;&nbsp; <i class="fa fa-trash-o text-danger" onclick="DeleteForward(' + val.for_id + ')"> Delete </i> </span>  &nbsp;&nbsp;  <i class="fa fa-user-o text-primary"> From - <b> <a href="/Admin/UserDetails/'+val.user_id+'/'+val.fullname+'" target="_blank"> '+val.fullname+' </a> </b></i>  <br/> ';
               
                    html += ' <br><small>';

                    html += ' <span id="tag" title="Customer\'s company"> <i class="fa fa-university">  </i> ' + val.company + ' </span> &nbsp;';
                    html += ' <span id="tag" title="Customer\'s fullname"> <i class="fa fa-user">  </i> ' + val.customer + ' </span> &nbsp;';
                    html += ' <span id="tag" title="Customer\'s phone number"> <i class="fa fa-phone">  </i> ' + val.phone + ' </span> &nbsp;';
                    html += ' <span id="tag" title="Customer\'s email address"> <i class="fa fa-at">  </i> ' + val.email + ' </span> &nbsp;';
                    html += ' <span id="tag" title="Customer\'s location"> <i class="fa fa-map-marker">  </i> ' + val.state + ' | ' + val.lga + ' </span> &nbsp;';
                    html += ' <span id="tag" title="Complain date"> <i class="fa fa-calendar text-danger">  </i> ' + val.date_c + ' </span> &nbsp;';
                    html += ' <span id="tag" title="Customer\'s call country"> <i class="fa fa-globe">  </i> ' + val.country + ' </span> &nbsp;';
                    html += ' <span id="tag" title="Complain option"> <i class="fa fa-cog text-info"> </i> ' + val.option + ' </span> &nbsp;';
                    html += ' <span id="tag" title="Complain status"> <i class="fa fa-lightbulb-o text-warning"> </i> ' + val.status + '</span> &nbsp;';

                    html += '</small></div>';
                    html += '<br />  <br />';

                    div.append(html);
                });

                btn.text('Load more');
                //$("html, body").animate({ scrollTop: $(document).height() }, 2000);
            }
        });
    });



    // Set as active function
    function toggleStatus(id) {
        $.post('/REVIEWER/Activate', { 'tel_id': id }, function (data) {
            if (data == "done") {
                alert("Success!!! \n\n This complain is now active.");
            }
            else {
                alert("Information!!! \n\n" + data);
            }
        });
    }

    // preceeding active function event
    $("#SetActive").on('click', function () {
        var id = $(".DivComplain").attr("id");
        toggleStatus(id);
    });


    // load answers
    $("#DivLoadAnswers").show(function () {
        var id = $(".DivComplain").attr("id");
        GetAnswers(id);
    });


    // FAQ search button
    $("#btnFSearch").on('click', function (e) {

        var text = $("#txtFSearch").val().trim();

        if (text === "") {
            return;
        }
        else {
            $("#Load").fadeIn('fast');

            e.preventDefault();

            var arr = text.split(" ");
            var str = "";

            for (var i = 0; i < arr.length; ++i) {
                str += arr[i] + " OR ";
            }

            var last = str.lastIndexOf("OR");
            str = str.substring(0, last);

            $.ajax({
                cache: false,
                type: "GET",
                url: "/Admin/FullText",
                data: { "contains": str, "text": text },
                success: function (data) {
                   
                    $("#SearchResults").html(data);
                    $("#Load").fadeOut('fast');
                }      
            });
        }          
    });


    function user_search(option, text, locate) {
        $.ajax({
            cache: false,
            type: "GET",
            url: "/Admin/"+locate,
            data:{'option':option, 'text':text},
            success: function (datas) {
                $("#OperationDiv").html(datas);
                $("#Load").fadeOut('fast');
            }
        });
    }


    function complain_list(option, text, limit, row, val) {
        $.ajax({
            cache: false,
            type: "GET",
            url: "/Admin/ComplainList",
            data: { 'option': option, 'text': text, 'limit' : limit, 'row' : row, 'val' : val},
            success: function (datas) {
                $("#OperationDiv").html(datas);
                $("#Load").fadeOut('fast');
            }
        });
    }


    $("#ListUser").on('click', function () {
        $("#SearchingOptions").hide();
        $("#OperationDiv").html();
        $("#ListUser").css('color', '#c7254e');
        $("#ListComplain").css('color', '');
        $("#ListForward").css('color', '');
        $("#Load").fadeIn('fast');

        user_search('ALL',null,"Users");
    });


    $("#txtSearchUser").on('keyup', function (e) {
        e.preventDefault();
        $("#btnTxtSearch").click();
    });

    $("#ListComplain").on('click', function () {
        $("#SearchingOptions").show();
        $("#OperationDiv").html();

        $("#ListUser").css('color', '');
        $("#ListForward").css('color', '');
        $("#ListComplain").css('color', '#c7254e');
        $("#Load").fadeIn('fast');
        $("#btnTxtSearch").click();

    });

    // geting forwarded list
    $("#ListForward").on('click', function () {
        $("#SearchingOptions").hide();
        $("#OperationDiv").html();
        $("#ListUser").css('color', '');
        $("#ListComplain").css('color', '');
        $("#ListForward").css('color', '#c7254e');
        $("#Load").fadeIn('fast');

        $.ajax({
            cache: false,
            type: "GET",
            url: "/Admin/Get_Forward",
            data: { 'num': 5 },
            success: function (datas) {
                $("#OperationDiv").html(datas);
                $("#Load").fadeOut('fast');
            }
        });
    });


    // admin complain search button
    $("#btnTxtSearch").on('click', function () {
        $("#Load").fadeIn('fast');
        var txt = $("#txtSearchUser");

        if (txt.val() === "") {
            $("#allInfo").text("Search result for ALL");
            $("#txtValue").val('ALL');
            complain_list("ALL", null, $("#Limit").val(), "NOT", null);
        }
        else {
            $("#allInfo").text("Search result for " + txt.val());
            $("#txtValue").val('Search');
            $("#txtValue2").val(txt.val());
            complain_list("Search", txt.val(), $("#Limit").val(), "NOT", null);
        }

       
    });


    // gets all records done by a user.

    $("#userSelect").on('change', function () {
        $("#Load").fadeIn('fast');
        var date1 = $("#datetimepicker_dark");
        var date2 = $("#datetimepicker_dark2");
        var user = $("#userSelect");

        var id = $("#userSelect").val().split("|");

        if (user.val() !== "" && (date1.val() !== "" && date2.val() !== "")) {
            $("#allInfo").text("Search result for " + id[0] + " Between " + date1.val() + " and " + date2.val());
            $("#txtValue").val("Users3");
            $("#txtValue2").val(id[1] + '|' + date1.val() + '|' + date2.val());
            complain_list("Users3", $("#txtValue2").val(), $("#Limit").val(), null, null);
        }
       else if (user.val() !== "" && date1.val() === "") {
            $("#allInfo").text("Search result for Agent (All for this agent)" + id[0]);
            $("#txtValue").val("Users1");
            $("#txtValue2").val(id[1]);
            complain_list("Users1", $("#txtValue2").val(), $("#Limit").val(), null, null);
        }
        else if (user.val() !== "" && date1.val() !== "") {
            $("#allInfo").text("Search result for " + id[0] + " On the date " + date1.val());
            $("#txtValue").val("Users2");
            $("#txtValue2").val(id[1] + '|' + date1.val());
            complain_list("Users2", $("#txtValue2").val(), $("#Limit").val(), null, null);
        }
        
    });


    $("#option_status").on('change', function () {
        $("#txtValue").val('Status');
        $("#txtValue2").val($("#option_status").val());
        complain_list("Status", $("#option_status").val(), $("#Limit").val(), "NOT", null);
    });

    $("#Limit").on('change', function () {
        $("#Load").fadeIn('fast');
        complain_list($("#txtValue").val(), $("#txtValue2").val(), $("#Limit").val(), "NOT", null);
    });


    $("#btnUser").on('click', function () {
    // user list 
       
    });




    // load sent forwarded complains for an agent
    $("#DivFrom").show(function () {
        $("#Load").fadeIn('fast');
        $.ajax({
            cache: false,
            type: "GET",
            url: "/Admin/LoadSentComplain",
            success: function (datas) {
                $("#DivFrom").html(datas);
                $("#Load").fadeOut('fast');
            }
        });
    });



    // getting list of platform
    $("#txtPlatform").ready(function () {
        var html = '';
        
        $("#txtPlatform").html("");

        $.getJSON('/REVIEWER/GetPlatform',{}, function (datas) {
            $("#txtPlatform").append("<option>Select Platform</option>");
            $.each(datas, function (key, val) {
                html += '<option>' + val.platform + '</option>';
            });
            $("#txtPlatform").append(html);
        });
    });


    // getting list of platform
    $("#txtPlat").ready(function () {
        var html = '';

        $("#txtPlat").html("");

        $.getJSON('/REVIEWER/GetPlatform', {}, function (datas) {
            $("#txtPlat").append("<option>All</option>");
            $.each(datas, function (key, val) {
                html += '<option>' + val.platform + '</option>';
            });
            $("#txtPlat").append(html);
        });
    });



    // getting list of users for report
    $("#SelectUsers").ready(function () {
        var html = '';
        $("#SelectUsers").html("");

        $.getJSON('/REVIEWER/GetUsers', {}, function (datas) {
            $("#SelectUsers").append("<option>All</option>");
            $.each(datas, function (key, val) {
                html += '<option value='+val.email+'>' + val.name + '</option>';
            });
            $("#SelectUsers").append(html);
        });
    });



    // getting list of platform
    $("#txtPlatform2").ready(function () {
        var html = '';

        $("#txtPlatform").html("");

        $.getJSON('/REVIEWER/GetPlatform', {}, function (datas) {
            $("#txtPlatform2").append("<option>All</option>");
            $.each(datas, function (key, val) {
                html += '<option>' + val.platform + '</option>';
            });
            $("#txtPlatform2").append(html);
        });
    });


    
    // navigate next for active complain
    $("#btnNextA").on('click', function () {
        var id = $(".divA:last").attr('data-id');
        $("#btnNextA").text('Loading...');

        $.ajax({
            cache: false,
            type: "GET",
            url: "/REVIEWER/GetMoreActiveComplain",
            data:{'more_id': id},
            success: function (datas) {

                if (datas.length == 0) {
                    $("#btnNextA").text('No more active complains');
                }
                else {
                    $("#DivActiveComplain").append(datas);
                    $("#btnNextA").text('Load more');
                    //$("html, body").animate({ scrollTop: $(document).height() }, 2000);
                }
            }
        });
    });

    // date picker 'from'
    $('#datetimepicker_dark').datetimepicker({
        theme: 'dark',
        lang: 'sn',
        timepicker: false,
        format: 'm/d/Y',
        formatDate: 'Y/m/d',
        maxDate: '+1970/01/02' 
    });


    // date picker 'from'
    $('#MyReportDate1').datetimepicker({
        theme: 'default',
        lang: 'sn',
        timepicker: false,
        format: 'm/d/Y',
        formatDate: 'Y/m/d',
    });

    // date picker 'from'
    $('#Date1').datetimepicker({
        theme: 'default',
        lang: 'sn',
        timepicker: false,
        format: 'm/d/Y',
        formatDate: 'Y/m/d',
    });

    // date picker 'from'
    $('#Date2').datetimepicker({
        theme: 'default',
        lang: 'sn',
        timepicker: false,
        format: 'm/d/Y',
        formatDate: 'Y/m/d',
    });


    // date picker 'from'
    $('#MyReportDate2').datetimepicker({
        theme: 'default',
        lang: 'sn',               
        timepicker: false,
        format: 'm/d/Y',
        formatDate: 'Y/m/d',
       
    });



    // date picker 'to'
    $('#datetimepicker_dark2').datetimepicker({
        theme: 'dark',
        lang: 'ch',
        timepicker: false,
        format: 'm/d/Y',
        formatDate: 'Y/m/d',
        maxDate: '+1970/01/02'
    });



    //change password for agent 
    $("#btnChangePass").on("click", function (e) {
        e.preventDefault();

        var email = $("#cEmail"),
            oldpass = $("#cOldPass"),
            newpass = $("#cNewPass"),
            compass = $("#cConfirmPass"),
            info = $("#cInfo");

        if (email.val() === "" || oldpass.val() === "" || newpass.val() === "" || compass.val() === "") {
            ErrorMessage(info, "All fields are required.");
        }
        else if(oldpass.val().length <= 7 || newpass.val().length <= 7){
            ErrorMessage(info, "Password too short. Password should be more than 7 characters.");
        }
        else if (compass.val() !== newpass.val()) {
            ErrorMessage(info, "New password and comfirm password does not match.");
        }
        else {

            var msg = confirm("Are you sure you want to chnage your password?");

            if (msg === true) {

                $.post("/REVIEWER/Manage", { 'email': email.val(), 'oldpass': oldpass.val(), 'newpass': newpass.val() }, function (data) {
                    if (data === "changed") {
                        SuccessMessage(info, "Your password has been changed.");

                        email.val('');
                        oldpass.val('');
                        newpass.val('');
                        compass.val('');
                    }
                    else {
                        ErrorMessage(info, data);
                    }
                });
            }
        }
    });



    /*
        Clearing the modal for forward
    */
    $("#userModal").on('hidden.bs.modal', function () {
        $("#txtUserID").val('');
        $("#txtForward").val('');
    });


    /*
    * Complains forwarded to a user.
    */
    var forward = $("#ForList").DataTable({

        "columns": [
                        
                { "data": "for_id", "name": "for_id" },
                { "data": "fullname", "name": "fullname" },
                { "data": "company", "name": "company" },
                { "data": "title", "name": "title" },
                { "data": "complain1", "name": "complain1" },
                { "data": "reason", "name": "reason" },
                { "data": "date", "name": "date" },
                {
                    "render": function (data, type, row) {
                        return '<a href="/Admin/Complain/' + row['tel_id'] + '/' + row['title'] + '" target="_blank" class="btn btn-primary btn-xs"> See more </a>';
                    }
                },
                {
                    "render": function (data, type, row) {
                        return '<button class="btn btn-warning btn-xs" onclick="Mark(' + row['for_id'] + ')"> <i class="fa fa-check"> </i> Mark </button>';
                    }
                },
                
        ],

        "processing": true, 
        "serverSide": true, 
        "filter": false, 
        "orderMulti": false,
        "ajax": {
            "url": "/Admin/RequetsToYou",
            "type": "POST",
        },

        responsive: true,

        "columnDefs": [
            {
                "targets": [0],
                "visible": false,
                "searchable": false
            },
            {
                "targets": [7],
                "orderable": false
            },
            {
                "targets": [8],
                "orderable": false
            }
        ]
            
    });



    /*
    *  Complain a user sent
    */
    var Sent = $("#SentList").DataTable({

        "columns": [

                { "data": "for_id", "name": "for_id" },
                { "data": "fullname", "name": "fullname" },
                { "data": "company", "name": "company" },
                { "data": "title", "name": "title" },
                { "data": "complain1", "name": "complain1" },
                { "data": "reason", "name": "reason" },
                { "data": "date", "name": "date" },
                {
                    "render": function (data, type, row) {
                        return '<a href="/Admin/Complain/' + row['tel_id'] + '/' + row['title'] + '" target="_blank" class="btn btn-primary btn-xs"> See more </a>';
                    }
                },
                {
                    "render": function (data, type, row) {
                        return '<button class="btn btn-warning btn-xs" onclick="Mark(' + row['for_id'] + ')"> <i class="fa fa-check"> </i> Mark </button>';
                    }
                },

        ],

        "processing": true,
        "serverSide": true,
        "filter": false,
        "orderMulti": false,
        "ajax": {
            "url": "/Admin/RequestYouSent",
            "type": "POST",
        },

        responsive: true,

        "columnDefs": [
            {
                "targets": [0],
                "visible": false,
                "searchable": false
            },
            {
                "targets": [7],
                "orderable": false
            },
            {
                "targets": [8],
                "orderable": false
            }
        ]

    });



    /*
    *  Complain Reporting
    */
    var complain = $("#ComplainList").DataTable({

        "autoWidth": true,
        "lengthMenu": [[10, 100, 250, 500, 1000], [10, 100, 250, 500, 1000,]],

        "columns": [

                { "data": "tel_id", "name": "tel_id"},
                { "data": "company", "name": "company", "searchable":true },
                { "data": "c_fullname", "name": "c_fullname" },
                { "data": "c_phone", "name": "c_phone" },
                { "data": "c_state", "name": "c_state" },
                { "data": "c_lga", "name": "c_lga" },
                { "data": "title", "name": "title" },
                { "data": "complain1", "name": "complain1" },
                { "data": "inc_option", "name": "inc_option" },
                { "data": "inc_status", "name": "inc_status" },
                { "data": "fullname", "name": "fullname" },
                { "data": "platform", "name": "platform" },
                { "data": "date", "name": "date" },
                {
                    "render": function (data, type, row) {
                        return '<a href="/Admin/Complain/' + row['tel_id'] + '/' + row['title'] + '" target="_blank" class="btn btn-primary btn-xs"> See more </a>';
                    }
                },
        ],

        "processing": true,
        "serverSide": true,
        
        "orderMulti": false,
        "ajax": {
            "url": "/Admin/GetComplainReport",
            "type": "POST",
            "data": function (d) {
                d.MyReportDate1 = $('#MyReportDate1').val();
                d.MyReportDate2 = $('#MyReportDate2').val();
            }
        },

       
        "columnDefs": [
            {
                "targets": [0],
                "visible": false,
                "searchable": false
            },
            {
                "targets": [12],
                "searchable": false,
                "orderable": false
            }
        ]

    });

 

        // Event listener to the two range filtering inputs to redraw on input
    $('#MyReportDate1, #MyReportDate2, #txtPlatform2').change(function () {

        if ($("#txtPlatform").val() === "" || $("#txtPlatform").val() === "Select Platform") {
            return;
        }
        else {

            $.ajax({
                cache: false,
                type: "GET",
                url: "/Admin/GetMyReport",
                data: { 'MyReportDate1': $("#MyReportDate1").val(), 'MyReportDate2': $("#MyReportDate2").val(), 'txtPlatform': $("#txtPlatform2").val() },
                success: function (data) {

                    $("#MyReporting").html(data);

                }

            });
        }
        });


    $("#btnReportUser").on('click', function (e) {
        e.preventDefault();
        if ($("#txtPlat").val() === "") {
            return;
        }
        else {

            $.ajax({
                cache: false,
                type: "GET",
                url: "/Admin/GetReportUsers",
                data: { 'Date1': $("#Date1").val(), 'Date2': $("#Date2").val(), 'txtPlat': $("#txtPlat").val(), 'User': $('#SelectUsers').val() },
                success: function (data) {
                    $("#ReportingUser").html(data);
                }
            });
        }
    });


    // Event listener to the two range filtering inputs to redraw on input
    $('#SelectUsers, #Date1, #Date2, #txtPlat').change(function () {
       
    });


    


    function Print(div, title) {

        var contents = $(div).html();

        var frame1 = $('<iframe />');
        frame1[0].name = "frame1";
        frame1.css({ "position": "absolute", "top": "-1000000px" });
        $("body").append(frame1);
        var frameDoc = frame1[0].contentWindow ? frame1[0].contentWindow : frame1[0].contentDocument.document ? frame1[0].contentDocument.document : frame1[0].contentDocument;
        frameDoc.document.open();
        //Create a new HTML document.
        frameDoc.document.write('<html><head><title>' + title + '</title>');
        //Append the external CSS file.
        frameDoc.document.write('<link rel="stylesheet" type="text/css" href="/Content/bootstrap.css">');
        frameDoc.document.write('<link rel="stylesheet" type="text/css" href="/Content/MyStyle.css">');

        frameDoc.document.write('</head><body>');

        //Append the DIV contents.
        frameDoc.document.write(contents);
        frameDoc.document.write('</body></html>');
        frameDoc.document.close();
        setTimeout(function () {
            window.frames["frame1"].focus();
            window.frames["frame1"].print();
            frame1.remove();
        }, 500);
        return false;
    }


    // FUnction for printing

    function funcPrint(element_id) {

        var txtSearch = $('.dataTables_filter input').val();
        var searchValue = "";

        if (txtSearch === "") {
            searchValue = "Displayed report";
        }
        else {
            searchValue = txtSearch;
        }

        Print($("#"+ element_id), "Search result for " + searchValue);
    }



    // function for exporting to excel
    function funcExport(element_id, search) {

        var txtSearch = $('.dataTables_filter input').val();

        var srch = "";

        if (txtSearch === "") {
            srch = search;
        } else {
            srch = txtSearch;
        }

        $("#" + element_id).table2excel({

            name: "Worksheet Name",
            filename: "Exported report for " + srch + " " + Math.floor((Math.random() * 9999999) + 1000000),
        });

    }


   
    
    /*
    *   Printing button for Complain report.
    */
    $("#btnPrintComplain").on('click', function (e) {
        e.preventDefault();
        funcPrint("ComplainReport");
    });


    $("#btnPrintMyReport").on('click', function (e) {
        e.preventDefault();
        funcPrint("MyReporting");
    });

    $("#btnPrintReportUser").on('click', function (e) {
        e.preventDefault();
        funcPrint("ReportingUser");
    });


    /*
    *   Printing button for Forward report.
    */
    $("#btnPrintForward").on('click', function (e) {
        e.preventDefault();
        funcPrint("ForwardReport");
    });



    /*
   *   Printing button for Users report.
   */
    $("#btnPrintUsers").on('click', function (e) {
        e.preventDefault();
        funcPrint("UsersReport");
    });


    /*
     *   Printing button for Active complain report.
    */
    $("#btnPrintActive").on('click', function (e) {
        e.preventDefault();
        funcPrint("ActiveComplainReport");
    })




    /*
   *   Export button for forward report.
   */
    $("#btnExportForward").on('click', function (e) {
        e.preventDefault();
        funcExport("ForwardReport", "List of forwarded reports");
    });


    
    $("#btnExportMyReport").on('click', function (e) {
        e.preventDefault();
        funcExport("tableMyReport", "My Report");
    });


    $("#btnExportReportUser").on('click', function (e) {
        e.preventDefault();
        funcExport("tableReportUsers", "Users Report");
    });


  
    /*
    *   Export button for complain report.
    */
    $("#btnExportComplain").on('click', function (e) {
        e.preventDefault();
        funcExport("ComplainReport", "List of Complain reports");
    });




    /*
    *   Export button for Users report.
    */
    $("#btnExportUsers").on('click', function (e) {
        e.preventDefault();
        funcExport("UsersReport", "List of Users.");
    });


    /*
    *   Export button for Active compalin report.
    */
    $("#btnExportActive").on('click', function (e) {
        e.preventDefault();
        funcExport("ActiveComplainReport", "Report for active complain.");
    });
    



    /*
    *   Forward reporting
    */
    var forwardReport = $("#ForwardList").DataTable({

        "lengthMenu": [[10, 25, 50, 100], [10, 25, 50, 100]],

        "columns": [

                { "data": "for_id", "name": "for_id" },
                { "data": "company", "name": "company" },
                { "data": "title", "name": "title" },
                { "data": "userFrom", "name": "userFrom" },
                { "data": "userTo", "name": "userTo" },
                { "data": "reason", "name": "reason" },
                { "data": "solved", "name": "solved" },
                { "data": "for_date", "name": "for_date" },
                {
                    "render": function (data, type, row) {
                        return '<a href="/Admin/Complain/' + row['tel_id'] + '/' + row['title'] + '" target="_blank" class="btn btn-primary btn-xs"> See more </a>';
                    }
                },
                {
                    "render": function (data, type, row) {
                        return '<button class="btn btn-warning btn-xs" onclick="Mark(' + row['for_id'] + ')"> <i class="fa fa-check"> </i> Mark </button>';
                    }
                },
                {
                    "render": function (data, type, row) {
                        return '<button class="btn btn-danger btn-xs" onclick="DeleteForward(' + row['for_id'] + ')"> <i class="fa fa-trash-o"> </i> Delete </button>';
                    }
                },
        ],

        "processing": true,
        "serverSide": true,
        "orderMulti": false,
        "ajax": {
            "url": "/Admin/GetRequestReport",
            "type": "POST",
        },

        responsive: true,

        "columnDefs": [
            {
                "targets": [0],
                "visible": false,
                "searchable": false
            },
            {
                "targets": [8],
                "orderable": false,
                "searchable": false
            },
            {
                "targets": [9],
                "orderable": false,
                "searchable": false
            },
            {
                "targets": [10],
                "orderable": false,
                "searchable": false
            }
        ]

    });



    /*
    * Users List reporting
    */
    var usersReport = $("#UsersList").DataTable({

        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],

        "columns": [

                { "data": "user_id", "name": "user_id" },
                { "data": "fullname", "name": "fullname" },
                { "data": "email", "name": "email" },
                { "data": "gender", "name": "gender" },
                { "data": "phone", "name": "phone" },
                { "data": "role", "name": "role" },
                {"data": "status", "name":"status"},
                { "data": "date", "name": "date" },
                {
                    "render": function (data, type, row) {
                        return '<a href="/Admin/UserDetails/' + row['user_id'] + '/' + row['fullname'] + '" target="_blank" class="btn btn-primary btn-xs"> See more </a>';
                    }
                }
        ],

        "processing": true,
        "serverSide": true,
        "orderMulti": false,
        "ajax": {
            "url": "/Admin/GetUsersReport",
            "type": "POST",
        },

        responsive: true,

        "columnDefs": [
            {
                "targets": [0],
                "visible": false,
                "searchable": false
            },
            {
                "targets": [7],
                "orderable": false,
                "searchable": false
            }
        ]

    });



    /*
    * Action to update Agent profile
    */
    $("#UpdateUSerForm").on('submit', function (e) {
        e.preventDefault();

        var msg = confirm("Are you sure you want to update this user information?");

        if (msg === true) {

            var formData = new FormData($(this)[0]);

            $.ajax({
                type: "POST",
                url: '/Admin/UpdateUser',
                data: formData,
                async: false,
                cache: false,
                contentType: false,
                processData: false,
                success: function (response) {
                    if ($.trim(response) === "Done") {
                        SuccessMessage("#EditMsg", "User information successfully updated.");
                        alert("User information successfully updated.");
                        location.reload(false);
                    }
                    else {
                        ErrorMessage("#EditMsg", response);
                    }
                },
                error: function (error) {
                    ErrorMessage("#EditMsg", "An Error was encountered trying to update this user information."); 
                }
            });
        }
    });




    /*
    *  User Activation
    */
    $("#btnActivateUser").on('click', function (e) {
        e.preventDefault();

        var user_id = $("#UserID").val();

        var msg = confirm("Are you sure you want to Activate this user?");

        if (msg === true) {
            $.post("/Admin/ActivateUser", { "userID": user_id }, function (response) {
                if ($.trim(response) === "Activated") {
                    SuccessMessage("#EditMsg", "User Account Activated.");
                    $("#userStatus").load(document.URL + ' #userStatus');
                }
                else {
                    ErrorMessage("#EditMsg", response);
                }
            });
        }
    });


    /*
    * Deactivating a user 
    */
    $("#btnDeactivateUser").on('click', function (e) {
        e.preventDefault();

        var user_id = $("#UserID").val();

        var msg = confirm("Are you sure you want to Dectivate this user?");

        if (msg === true) {
            $.post("/Admin/DeactivateUser", { "userID": user_id }, function (response) {
                if ($.trim(response) === "Deactivated") {
                    SuccessMessage("#EditMsg", "User Account Dectivated.");
                    $("#userStatus").load(document.URL + ' #userStatus');
                }
                else {
                    ErrorMessage("#EditMsg", response);
                }
            });
        }
    });



   /*
  * Deleting a user
  */
    $("#btnDeleteUser").on('click', function (e) {
        e.preventDefault();

        var user_id = $("#UserID").val();

        var msg = confirm("Are you sure you want to Delete this user?");

        if (msg === true) {
            $.post("/Admin/DeleteUser", { "userID": user_id }, function (response) {
                if ($.trim(response) === "Deleted") {
                    alert("User Deleted!");
                    window.location = "/Admin/UserReport";
                }
                else {
                    ErrorMessage("#EditMsg", response);
                }
            });
        }
    });



    // password validation with lib.
    $('#btnResetPassword').attr('disabled', true);
    $('#ResetPass1').password().on('password.score', function (e, score) {
        if (score > 70) {
            $('#btnResetPassword').removeAttr('disabled');
        } else {
            $('#btnResetPassword').attr('disabled', true);
        }
    });


    //Admin Changing password for user
    $("#btnResetPassword").on('click', function (e) {
        e.preventDefault();

        var pass1 = $("#ResetPass1");
        var pass2 = $("#ResetPass2");
        var user_id = $("#UserID").val();

        if (pass1.val() !== pass2.val()) {
            ErrorMessage("#PasswordInfo", "Password does not match.");
        }
        else {
            var msg = confirm("Are you sure you want to change this user password?");

            if (msg === true) {
                $.post("/Admin/ResetPassword", { "userID": user_id, "password": pass1.val() }, function (response) {
                    if ($.trim(response) === "Changed") {
                        SuccessMessage("#PasswordInfo", "User password changed!");
                        $("#btnClearReset").click();
                    }
                    else {
                        ErrorMessage("#PasswordInfo", response);
                    }
                });
            }
        }
    });


    // button click to save platform
    $("#btnPlatform").on('click', function (e) {
        e.preventDefault();

        var txtPlatform = $('#txtPlatform');

        if (txtPlatform.val() === "") {
            alert("Please enter the platform to save.");
        }
        else {
            var msg = confirm("Are you sure you want to save this platform?");

            if (msg === true) {
                $.getJSON("/REVIEWER/SavePlatform", { "txtPlatform": txtPlatform.val() }, function (response) {
                    if (response === "Platform Saved") {
                        alert("Platform successfully saved.");
                        txtPlatform.val('');
                    }
                    else {
                        alert(response);
                    }
                });
            }
        }
    });


    // button clear reset
    $("#btnClearReset").on('click', function (e) {
        e.preventDefault();

       $("#ResetPass1").val('');
       $("#ResetPass2").val('');
    });




    /*
    * Active complain listing
    */
    var Active = $("#ActiveList").DataTable({

        "lengthMenu": [[10, 100, 250, 500, 1000], [10, 100, 250, 500, 1000, ]],

        "columns": [

                { "data": "tel_id", "name": "tel_id" },
                { "data": "company", "name": "company" },
                { "data": "c_fullname", "name": "c_fullname" },
                { "data": "title", "name": "title" },
                { "data": "complain1", "name": "complain1" },
                { "data": "inc_option", "name": "inc_option" },
                { "data": "fullname", "name": "fullname" },
                { "data": "platform", "name": "platform" },
                { "data": "date", "name": "date" },
                {
                    "render": function (data, type, row) {
                        return '<a href="/Admin/Complain/' + row['tel_id'] + '/' + row['title'] + '" target="_blank" class="btn btn-primary btn-xs"> See more </a>';
                    }
                },
                {
                    "render": function (data, type, row) {
                        return '<span data-toggle="modal" class="btn btn-xs btn-warning" onclick="get_users(' + row['tel_id'] + ')" data-target="#userModal"><i class="fa fa-mail-forward" title="Forward complain"> </i> Forward</span>';
                    }
                },

        ],

        "processing": true,
        "serverSide": true,
        "filter": false,
        "orderMulti": false,
        "ajax": {
            "url": "/REVIEWER/ActiveComplains",
            "type": "POST",
        },

        responsive: true,

        "columnDefs": [
            {
                "targets": [0],
                "visible": false,
                "orderable": false
            },
            {
                "targets": [8],
                "orderable": false
            },
            {
                "targets": [9],
                "orderable": false
            }
        ]

    });


});





    
