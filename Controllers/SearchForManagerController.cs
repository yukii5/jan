using Achievement.SearchForManager.Models;
using AchievementAPI.Models;
using App.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReportForm.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace Achievement.SearchForManager.Controllers
{
    [Authorize]
    public class SearchForManagerController : Controller
    {

        #region Logger
        /// <summary>
        /// Log Writer
        /// </summary>
        /// <remarks>
        /// ※ ASP.NET MVC Core では、Logger も DIで使用する。
        /// </remarks>
        private LogWriter _logWriter = null;
        #endregion

        #region フィールド
        /// <summary>
        /// HTTP Client
        /// </summary>
        private static readonly HttpClient _client = new HttpClient();
        #endregion

        #region コンストラクタ
        public SearchForManagerController(IConfiguration config, ILogger<SearchForManagerController> logger)
        {
            // グローバルオブジェクト設定
            Global.Configuration = config;
            Global.Logger = logger;

            // Log Writer
            _logWriter = new LogWriter();

            Init();
        }
        #endregion

        #region パブリックメソッド

        #region 検索欄表示
        /// <summary>
        /// 検索欄表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // 開始ログ
                _logWriter.WriteStartLog(Request, "[操作]検索一覧 - 検索欄表示");

                // 権限チェック
                SecureLogic secure = new SecureLogic(HttpContext, _logWriter);

                // APIURL作成
                string baseUri = Global.GetConfiguration(APP_CONSTS.SECTION_APPLICATION_SETTINGS, APP_CONSTS.APP_SETTING_API_BASE_URI);

                string officeUri = baseUri
                                + Global.GetConfiguration(APP_CONSTS.SECTION_API_URI, APP_CONSTS.ACHIEVEMENT.API_SEARCHES_GET_OFFICELIST);
                string prefUri = baseUri
                                + Global.GetConfiguration(APP_CONSTS.SECTION_API_URI, APP_CONSTS.ACHIEVEMENT.API_SEARCHES_GET_PREFLIST);
                string branchUri = baseUri
                                + Global.GetConfiguration(APP_CONSTS.SECTION_API_URI, APP_CONSTS.ACHIEVEMENT.API_SEARCHES_GET_BRANCHLIST);
                string userUri = baseUri
                                + Global.GetConfiguration(APP_CONSTS.SECTION_API_URI, APP_CONSTS.ACHIEVEMENT.API_SEARCHES_GET_SELECTUSER);

                SearchForManagerModel searchModel = new SearchForManagerModel();
                

                // 職種権限による検索範囲取得
                searchModel.Range = secure.CheckJobCode();

                // 検索範囲別初期クエリ追加
                switch (searchModel.Range)
                {
                    case SecureLogic.SEARCH_RANGE.MYSELF:
                        officeUri = officeUri + "?OfficeCode=" + Global.User.OfficeCode;
                        prefUri = prefUri + "?PrefCode=" + Global.User.PrefCode;
                        branchUri = branchUri + "?BranchCode=" + Global.User.BranchCode;
                        userUri = userUri + "?UserID=" + Global.User.UserId;
                        searchModel.SearchArg = new SearchManagerRequestModel();
                        searchModel.SearchArg.OfficeCode = Global.User.OfficeCode;
                        searchModel.SearchArg.PrefCode = Global.User.PrefCode;
                        searchModel.SearchArg.BranchCode = Global.User.BranchCode;
                        break;
                    case SecureLogic.SEARCH_RANGE.BRANCH:
                        officeUri = officeUri + "?OfficeCode=" + Global.User.OfficeCode;
                        prefUri = prefUri + "?PrefCode=" + Global.User.PrefCode;
                        branchUri = branchUri + "?BranchCode=" + Global.User.BranchCode;
                        searchModel.SearchArg = new SearchManagerRequestModel();
                        searchModel.SearchArg.OfficeCode = Global.User.OfficeCode;
                        searchModel.SearchArg.PrefCode = Global.User.PrefCode;
                        searchModel.SearchArg.BranchCode = Global.User.BranchCode;
                        break;
                    case SecureLogic.SEARCH_RANGE.PREF:
                        officeUri = officeUri + "?OfficeCode=" + Global.User.OfficeCode;
                        prefUri = prefUri + "?PrefCode=" + Global.User.PrefCode;
                        searchModel.SearchArg = new SearchManagerRequestModel();
                        searchModel.SearchArg.OfficeCode = Global.User.OfficeCode;
                        searchModel.SearchArg.PrefCode = Global.User.PrefCode;
                        break;
                    case SecureLogic.SEARCH_RANGE.OFFICE:
                        officeUri = officeUri + "?OfficeCode=" + Global.User.OfficeCode;
                        searchModel.SearchArg = new SearchManagerRequestModel();
                        searchModel.SearchArg.OfficeCode = Global.User.OfficeCode;
                        break;
                }

                // APIURL設定
                searchModel.SearchApiUriList = new Dictionary<string, string>
                {
                    {"office", officeUri },
                    {"pref", prefUri },
                    {"branch", branchUri },
                    {"user", userUri }
                };

                if (searchModel.Range == SecureLogic.SEARCH_RANGE.NONE)
                {
                    // 検索範囲なし
                    searchModel.BranchList = new();
                    searchModel.OfficeList = new();
                    searchModel.PrefList = new();
                    searchModel.UserList = new();
                    searchModel.SearchLog = new();
                    return View(searchModel);
                }

                // 検索履歴取得
                SetSessionData(searchModel);

                // 選択項目取得
                await ExecuteAPI(searchModel);


                return View(searchModel);

            }
            catch
            {
                throw;
            }
            finally
            {
                // 終了ログ
                _logWriter.WriteEndLog(Request, "[操作]検索一覧 - 検索欄表示");
            }
        }
        #endregion

        #region 検索結果表示
        /// <summary>
        /// ビューコンポーネント呼び出し用アクション
        /// </summary>
        /// <param name="param">パラメータ</param>
        /// <returns>ビューコンポーネント</returns>
        [HttpGet]
        public IActionResult List(SearchManagerRequestModel param)
        {

            if (param != null)
            {
                HttpContext.Session.SetInt32("search", 1);
            }
            if (param.OfficeCode != null)
            {
                HttpContext.Session.SetString("officeManager", param.OfficeCode);
            }
            else
            {
                HttpContext.Session.Remove("officeManager");
            }
            if (param.PrefCode != null)
            {
                HttpContext.Session.SetString("prefManager", param.PrefCode);
            }
            else
            {
                HttpContext.Session.Remove("prefManager");
            }
            if (param.BranchCode != null)
            {
                HttpContext.Session.SetString("branchManager", param.BranchCode);
            }
            else
            {
                HttpContext.Session.Remove("branchManager");
            }
            if (param.Month != null)
            {
                HttpContext.Session.SetString("monthManager", param.Month);
            }
            else
            {
                HttpContext.Session.Remove("monthManager");
            }
            return ViewComponent("SearchManagerList", param);
        }
        #endregion

        #endregion

        #region プライベートメソッド

        #region 初期化
        /// <summary>
        /// 初期化
        /// </summary>
        /// <remarks>コンストラクタから呼ぶ</remarks>
        private void Init()
        {
            // リクエストのタイムアウト値を appsettings.json の設定値に変更
            double timeout = 0d;
            double.TryParse(Global.GetConfiguration(APP_CONSTS.SECTION_APPLICATION_SETTINGS,
                                                    APP_CONSTS.APP_SETTING_REQUEST_TIMEOUT), out timeout);
            if (_client.Timeout != TimeSpan.FromSeconds(timeout))
            {
                // ※ この if文は、2回目以降の設定し直しは不可なので、
                //    最初の1回目のみ設定するようにするため。
                _client.Timeout = TimeSpan.FromSeconds(timeout);
            }
        }
        #endregion

        #region セッション情報設定
        /// <summary>
        /// セッション情報から過去の検索条件を設定
        /// </summary>
        /// <param name="searchModel">ビューモデル</param>
        private void SetSessionData(SearchForManagerModel searchModel)
        {
            searchModel.SearchLog = new Dictionary<string, string>();
            if (HttpContext.Session.GetInt32("search") == 1)
            {
                if (searchModel.SearchArg == null)
                {
                    searchModel.SearchArg = new SearchManagerRequestModel();
                }

                if (HttpContext.Session.GetString("officeManager") != null && searchModel.SearchArg.OfficeCode == null)
                {
                    searchModel.SearchLog.Add("officeManager", HttpContext.Session.GetString("officeManager"));
                    searchModel.SearchArg.OfficeCode = searchModel.SearchLog["officeManager"];
                }
                if (HttpContext.Session.GetString("prefManager") != null && searchModel.SearchArg.PrefCode == null)
                {
                    searchModel.SearchLog.Add("prefManager", HttpContext.Session.GetString("prefManager"));
                    searchModel.SearchArg.PrefCode = searchModel.SearchLog["prefManager"];
                }
                if (HttpContext.Session.GetString("branchManager") != null && searchModel.SearchArg.BranchCode == null)
                {
                    searchModel.SearchLog.Add("branchManager", HttpContext.Session.GetString("branchManager"));
                    searchModel.SearchArg.BranchCode = searchModel.SearchLog["branchManager"];
                }
                if (HttpContext.Session.GetString("monthManager") != null && searchModel.SearchArg.Month == null)
                {
                    searchModel.SearchLog.Add("monthManager", HttpContext.Session.GetString("monthManager"));
                    searchModel.SearchArg.Month = searchModel.SearchLog["monthManager"];
                }
            }
        }
        #endregion

        #region 検索欄選択項目取得API実行
        /// <summary>
        /// 検索欄に設定する選択項目を取得
        /// </summary>
        /// <param name="searchModel">ビューモデル</param>
        /// <returns>実行結果</returns>
        private async Task ExecuteAPI(SearchForManagerModel searchModel)
        {
            try
            {
                searchModel.OfficeList = new();
                searchModel.PrefList = new();
                searchModel.BranchList = new();

                string uri = searchModel.SearchApiUriList["office"];
                searchModel.OfficeList = await APIUtility.GetAsync<List<OfficeModel>>(_client, uri, HttpContext);
                if (searchModel.OfficeList == null)
                {
                    searchModel.OfficeList = new();
                    return;
                }

                uri = searchModel.SearchApiUriList["pref"];
                string query = null;
                query = CreateQueryString(searchModel, "pref");
                if (searchModel.PresetUri("pref"))
                {
                    searchModel.PrefList = await APIUtility.GetAsync<List<PrefModel>>(_client, uri, HttpContext);
                }
                else if (query != null)
                {
                    searchModel.PrefList = await APIUtility.GetAsync<List<PrefModel>>(_client, uri + query, HttpContext);
                }

                if (searchModel.PrefList == null)
                {
                    searchModel.PrefList = new();
                    return;
                }

                uri = searchModel.SearchApiUriList["branch"];
                query = null;
                query = CreateQueryString(searchModel, "branch");
                if (searchModel.PresetUri("branch"))
                {
                    searchModel.BranchList = await APIUtility.GetAsync<List<BranchModel>>(_client, uri, HttpContext);
                }
                else if (query != null)
                {
                    searchModel.BranchList = await APIUtility.GetAsync<List<BranchModel>>(_client, uri + query, HttpContext);
                }
                if (searchModel.BranchList == null)
                {
                    searchModel.BranchList = new();
                    return;
                }

                uri = searchModel.SearchApiUriList["user"];
                query = null;
                query = CreateQueryString(searchModel, "user");
                if (!searchModel.PresetUri("user") && query != null)
                {
                    searchModel.UserList = await APIUtility.GetAsync<List<SelectUserModel>>(_client, uri + query, HttpContext);
                }
                else
                {
                    searchModel.UserList = await APIUtility.GetAsync<List<SelectUserModel>>(_client, uri, HttpContext);
                }

                if (searchModel.UserList == null)
                {
                    searchModel.UserList = new();
                    return;
                }
            }
            catch
            {
                throw;
            }

        }
        #endregion

        #region クエリ作成
        /// <summary>
        /// 検索欄初期表示用クエリストリング作成
        /// </summary>
        /// <param name="searchModel">ビューモデル</param>
        /// <param name="key">作成する検索欄(office/pref/branch/user)</param>
        /// <returns>クエリストリング</returns>
        private string CreateQueryString(SearchForManagerModel searchModel, string key)
        {
            string query = "";

            if (searchModel.SearchArg == null)
            {
                return query;
            }

            switch (key)
            {
                case "pref":
                    if (searchModel.SearchArg.OfficeCode != null)
                    {
                        query += "?OfficeCode=" + searchModel.SearchArg.OfficeCode;
                    }
                    break;
                case "branch":
                    if (searchModel.SearchArg.OfficeCode != null)
                    {
                        query += "?OfficeCode=" + searchModel.SearchArg.OfficeCode;
                    }
                    if (searchModel.SearchArg.PrefCode != null)
                    {
                        query += query.Length > 0 ? "&" : "?";
                        query += "PrefCode=" + searchModel.SearchArg.PrefCode;
                    }
                    break;
                case "user":
                    if (searchModel.SearchArg.OfficeCode != null)
                    {
                        query += "?OfficeCode=" + searchModel.SearchArg.OfficeCode;
                    }
                    if (searchModel.SearchArg.PrefCode != null)
                    {
                        query += query.Length > 0 ? "&" : "?";
                        query += "PrefCode=" + searchModel.SearchArg.PrefCode;
                    }
                    if (searchModel.SearchArg.BranchCode != null)
                    {
                        query += query.Length > 0 ? "&" : "?";
                        query += "BranchCode=" + searchModel.SearchArg.BranchCode;
                    }
                    break;
            }
            return query;
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// 検索結果表示用ビューコンポーネント
    /// </summary>
    [Authorize]
    public class SearchManagerList : ViewComponent
    {

        #region Logger
        /// <summary>
        /// Log Writer
        /// </summary>
        /// <remarks>
        /// ※ ASP.NET MVC Core では、Logger も DIで使用する。
        /// </remarks>
        private LogWriter _logWriter = null;
        #endregion

        #region フィールド
        /// <summary>
        /// HTTP Client
        /// </summary>
        private static readonly HttpClient _client = new HttpClient();
        #endregion

        #region コンストラクタ
        public SearchManagerList(IConfiguration config, ILogger<SearchManagerList> logger)
        {
            // グローバルオブジェクト設定
            Global.Configuration = config;
            Global.Logger = logger;

            // Log Writer
            _logWriter = new LogWriter();

            Init();
        }
        #endregion

        #region パブリックメソッド

        #region 検索
        /// <summary>
        /// ユーザ検索
        /// </summary>
        /// <param name="param">パラメータ</param>
        /// <returns>検索結果</returns>
        [HttpGet]
        public async Task<IViewComponentResult> InvokeAsync([FromQuery] SearchManagerRequestModel param)
        {
            try
            {
                //開始ログ
                _logWriter.WriteStartLog(Request, "[操作]検索一覧 - 検索結果表示");

                SeachListModel model = new SeachListModel();

                if (param == null || string.IsNullOrEmpty(param.Month) ||
                    param.Month.Length != APP_CONSTS.ANNOTATION.FIX_LEN_YM || !DateTime.TryParse(DataUtility.ReformDateString(param.Month + "01"), out _))
                {
                    // 参照年月が空 or 正しい年月じゃない場合
                    // 検索結果 0件として返却
                    model.UserList = new List<SearchManagerModel>();
                }
                else if (param.OfficeCode != null)
                {
                    model.ReportMonth = param.Month;

                    string searchUri = Global.GetConfiguration(APP_CONSTS.SECTION_APPLICATION_SETTINGS, APP_CONSTS.APP_SETTING_API_BASE_URI)
                                    + Global.GetConfiguration(APP_CONSTS.SECTION_API_URI, APP_CONSTS.ACHIEVEMENT.API_SEARCHES_GET_USERLIST);

                    string query = "";
                    if (param.OfficeCode != null)
                    {
                        query += "?OC=" + param.OfficeCode;
                    }
                    if (param.PrefCode != null)
                    {
                        query += query.Length > 0 ? "&" : "?";
                        query += "PC=" + param.PrefCode;
                    }
                    if (param.BranchCode != null)
                    {
                        query += query.Length > 0 ? "&" : "?";
                        query += "BC=" + param.BranchCode;
                    }
                    searchUri += query;
                    model.UserList = await APIUtility.GetAsync<List<SearchManagerModel>>(_client, searchUri, HttpContext);
                }
                if (model.UserList == null)
                {
                    // 検索結果0件
                    model.UserList = new List<SearchManagerModel>();
                }
                //重複削除のための一時配列
                List<AchievementAPI.Models.SearchManagerModel> distinictModel = new List<AchievementAPI.Models.SearchManagerModel>();
                //重複比較用文字列
                string BCCount = "";
                //バッチコード重複削除
                for (int i = 0; i < model.UserList.Count; i++)
                {
                    if (!BCCount.Contains(model.UserList[i].BranchCode))
                    {
                        distinictModel.Add(model.UserList[i]);
                        BCCount += model.UserList[i].BranchCode + ",";
                    }
                }
                model.UserList = distinictModel;


                // 権限チェック
                SecureLogic secure = new SecureLogic(HttpContext, _logWriter);

                SearchForManagerModel AuthorityModel = new SearchForManagerModel();

                // 職種権限による検索範囲取得
                AuthorityModel.Range = secure.CheckJobCode();

                param.authority = AuthorityModel.Range;
                // 権限
                model.authority = param.authority;
                return View("~/Views/SearchForManager/_SearchForManagerList.cshtml", model);
            }
            catch
            {
                throw;
            }
            finally
            {
                // 終了ログ
                _logWriter.WriteEndLog(Request, "[操作]検索一覧 - 検索結果表示");
            }

        }
        #endregion

        #endregion

        #region プライベートメソッド

        #region 初期化
        /// <summary>
        /// 初期化
        /// </summary>
        /// <remarks>コンストラクタから呼ぶ</remarks>
        private void Init()
        {
            // リクエストのタイムアウト値を appsettings.json の設定値に変更
            double timeout = 0d;
            double.TryParse(Global.GetConfiguration(APP_CONSTS.SECTION_APPLICATION_SETTINGS,
                                                    APP_CONSTS.APP_SETTING_REQUEST_TIMEOUT), out timeout);
            if (_client.Timeout != TimeSpan.FromSeconds(timeout))
            {
                // ※ この if文は、2回目以降の設定し直しは不可なので、
                //    最初の1回目のみ設定するようにするため。
                _client.Timeout = TimeSpan.FromSeconds(timeout);
            }
        }
        #endregion

        #endregion
    }
}

