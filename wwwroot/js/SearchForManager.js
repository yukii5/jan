$(function () {
    let ranges = {
        'none': GetMetaContent('range_none'),
        'myself': GetMetaContent('range_myself'),
        'branch': GetMetaContent('range_branch'),
        'pref': GetMetaContent('range_pref'),
        'office': GetMetaContent('range_office'),
        'all': GetMetaContent('range_all'),
    };

    let range = GetMetaContent('range');

    let searchNum = $('#search_num').html().replace(/[0-9]+/, $('#i_listbody').attr('data-count'));
    $('#search_num').html(searchNum);


    $('.c_date').each(function () {
        if ($(this).val() != '') {
            var aryDate = $(this).val().match(/\d{4}|\d{2}|\d/g);
            if (aryDate != null && aryDate.length > 1) {
                $(this).val(aryDate.join('/'));
            }
        }
    });

    $('.c_date').on('focus input', function () {
        var fixVal = $(this).val().match(/\d/g);
        if (fixVal == null) {
            $(this).val('');
        } else if (fixVal.length > 0) {
            $(this).val(fixVal.join(''));
        }
    });

    $('.c_date').on('blur', function () {
        var aryDate = $(this).val().match(/\d{4}|\d{2}|\d/g);
        if (aryDate != null && aryDate.length > 1) {
            $(this).val(aryDate.join('/'));
        }
    });

    $('#i_office').on('change', function (e) {
        if ($(this).val() == "") {
            $('#i_user').val('');
            $('#i_branch').val('');
            $('#i_branch').prop('disabled', true);
            $('#i_pref').val('');
            $('#i_pref').prop('disabled', true);
            
            let uri = GetMetaContent('user_uri');
            ExecuteAjax(uri, 'get', null, true, true, GetSelectUserCallback);
        } else {
            $('#i_branch').val('');
            $('#i_branch').prop('disabled', true);
            $('#i_pref').val('');
            $('#i_pref').prop('disabled', false);
            let uri = GetMetaContent('pref_uri');
            uri += '?OfficeCode=' + $(this).val();
            ExecuteAjax(uri, 'get', null, true, true, GetSelectPrefCallback);
            uri = GetMetaContent('user_uri');
            uri += '?OfficeCode=' + $(this).val();
            ExecuteAjax(uri, 'get', null, true, true, GetSelectUserCallback);

        }
    });
    $('#i_pref').on('change', function (e) {
        if ($(this).val() == "") {
            $('#i_user').val('');
            $('#i_branch').val('');
            $('#i_branch').prop('disabled', true);
            let uri = GetMetaContent('user_uri');
            uri += '?OfficeCode=' + $('#i_office').val();
            ExecuteAjax(uri, 'get', null, true, true, GetSelectUserCallback);
        } else {
            $('#i_branch').val('');
            $('#i_branch').prop('disabled', false);
            let uri = GetMetaContent('branch_uri');
            uri += '?PrefCode=' + $(this).val();
            ExecuteAjax(uri, 'get', null, true, true, GetSelectBranchCallback);
            uri = GetMetaContent('user_uri');
            uri += '?PrefCode=' + $(this).val();
            ExecuteAjax(uri, 'get', null, true, true, GetSelectUserCallback);
        }
    });
    $('#i_branch').on('change', function (e) {
        if ($(this).val() == "") {
            $('#i_user').val('');
            let uri = GetMetaContent('user_uri');
            uri += '?PrefCode=' + $('#i_pref').val();
            ExecuteAjax(uri, 'get', null, true, true, GetSelectUserCallback);
        } else {
            let uri = GetMetaContent('user_uri');
            uri += '?BranchCode=' + $(this).val();
            ExecuteAjax(uri, 'get', null, true, true, GetSelectUserCallback);
        }
    });

    $('#i_load').on('click', function () {

        let query = [];

        if ($('#i_office').val()) {
            query.push('OC=' + $('#i_office').val());
        }
        if ($('#i_pref').val()) {
            query.push('PC=' + $('#i_pref').val());
        }
        if ($('#i_branch').val()) {
            query.push('BC=' + $('#i_branch').val());
        }
        if ($('#i_month').val()) {
            query.push('YM=' + $('#i_month').val().match(/\d/g).join(''));
        }
        let uri = 'SearchForManager/List?' + query.join('&');
        ExecuteAjax(uri, 'get', null, true, true, GetSearchCallback);
    });


    $('#i_clear').on('click', function () {
        let uri = null;
        switch (range) {
            case ranges['branch']:
                $('#i_user').val('');
                break;
            case ranges['pref']:
                $('#i_user').val('');
                $('#i_branch').val('');
                uri = GetMetaContent('user_uri');
                uri += '?PrefCode=' + $('#i_pref').val();
                ExecuteAjax(uri, 'get', null, true, true, GetSelectUserCallback);
                break;
            case ranges['office']:
                $('#i_branch').val('');
                $('#i_branch').prop('disabled', true);
                $('#i_pref').val('');
                RemoveOption('#i_branch');
                uri = GetMetaContent('user_uri');
                uri += '?OfficeCode=' + $('#i_office').val();
                ExecuteAjax(uri, 'get', null, true, true, GetSelectUserCallback);
                break;
            case ranges['all']:
                $('#i_branch').val('');
                $('#i_branch').prop('disabled', true);
                $('#i_pref').val('');
                $('#i_pref').prop('disabled', true);
                $('#i_office').val('');

                RemoveOption('#i_branch');
                RemoveOption('#i_pref');
                uri = GetMetaContent('user_uri');
                ExecuteAjax(uri, 'get', null, true, true, GetSelectUserCallback);
                break;
        }
    });

    $('#i_upload').on('click', function () {
        location.href = 'Capture';
    });

    $(document.body).on('click', '.c_user', function () {
        location.href = $(this).val();
    });
});

function GetSelectUserCallback(data) {
    RemoveOption('#i_user');
    $.each(data, function (i, v) {
        $('#i_user').append('<option value="' + v.userId + '">' + v.userName + '</option>');
    });
};
function GetSelectPrefCallback(data) {
    RemoveOption('#i_pref');
    $.each(data, function (i, v) {
        $('#i_pref').append('<option value="' + v.pref + '">' + v.prefName + '</option>');
    });
};
function GetSelectBranchCallback(data) {
    RemoveOption('#i_branch');
    $.each(data, function (i, v) {
        $('#i_branch').append('<option value="' + v.branch + '">' + v.branchName + '</option>');
    });
};

function GetSearchCallback(data) {
    $('#i_form').html(data);
    let result = $('#search_num').html().replace(/[0-9]+/, $('#i_listbody').attr('data-count'));
    $('#search_num').html(result);
};

function RemoveOption(id) {
    $(id + ' > option:not(:first-child)').remove();
};


window.onload = function () {
    let disable = {};
    //let elements = {};
    let DisaLink = '';

    // 引数の数だけループ
    for (let i = 0; i < arguments.length; i++) {
        DisaLink = arguments[i];
        disable = document.getElementById('DisaLink');
        //disable[DisaLink] = '';
    }
    let classNames = disable.className;
    //let disable = document.getElementsByClassName('c_link');
    console.log(disable);
    alert(disable.count);
}
