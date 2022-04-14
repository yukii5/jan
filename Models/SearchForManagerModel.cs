using AchievementAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportForm.Common;

namespace Achievement.SearchForManager.Models
{
    public class SearchForManagerModel
    {
        #region プロパティ
        /// <summary>
        /// 支社リスト
        /// </summary>
        public List<OfficeModel> OfficeList { get; set; } = null;

        /// <summary>
        /// 都道府県リスト
        /// </summary>
        public List<PrefModel> PrefList { get; set; } = null;

        /// <summary>
        /// 支店リスト
        /// </summary>
        public List<BranchModel> BranchList { get; set; } = null;

        ///// <summary>
        ///// 従業員リスト
        ///// </summary>
        public List<SelectUserModel> UserList { get; set; } = null;

        /// <summary>
        /// 検索範囲
        /// </summary>
        public SecureLogic.SEARCH_RANGE Range { get; set; } = SecureLogic.SEARCH_RANGE.NONE;

        /// <summary>
        /// APIURIリスト
        /// </summary>
        public Dictionary<string, string> SearchApiUriList { get; set; } = null;

        /// <summary>
        /// 検索履歴
        /// </summary>
        public Dictionary<string, string> SearchLog { get; set; } = null;

        /// <summary>
        /// 検索結果初期表示用パラメータ
        /// </summary>
        public SearchManagerRequestModel SearchArg { get; set; } = null;
        #endregion

        #region パブリックメソッド
        /// <summary>
        /// URIにクエリが含まれているかを判定する
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>判定結果</returns>
        public bool PresetUri(string key)
        {
            return SearchApiUriList[key].Contains("?");
        }

        /// <summary>
        /// selectedを設定する
        /// </summary>
        /// <param name="key">対象の検索欄</param>
        /// <param name="value">選択項目</param>
        /// <param name="range">検索範囲</param>
        /// <returns></returns>
        public string IsSelected(string key, string value, SecureLogic.SEARCH_RANGE range)
        {
            // 固定検索範囲
            if (range == SecureLogic.SEARCH_RANGE.MYSELF)
            {
                // 自分のみの場合はすべてselectedを返す
                return "selected";
            }
            if (range == SecureLogic.SEARCH_RANGE.BRANCH && (key == "officeManager" || key == "prefManager" || key == "branchManager"))
            {
                return "selected";
            }
            if (range == SecureLogic.SEARCH_RANGE.PREF && (key == "officeManager" || key == "prefManager"))
            {
                return "selected";
            }
            if (range == SecureLogic.SEARCH_RANGE.OFFICE && key == "officeManager")
            {
                return "selected";
            }

            // 可変検索範囲
            if (SearchLog.ContainsKey(key) && SearchLog[key] == value)
            {
                return "selected";
            }
            return "";
        }

        public string SetMonth()
        {
            if (SearchLog.ContainsKey("monthManager"))
            {
                return SearchLog["monthManager"];
            }
            else
            {
                return DateTime.Today.AddMonths(-1).ToString("yyyyMM");
            }
        }
        #endregion
    }
}
