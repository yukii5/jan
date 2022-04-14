
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AchievementAPI.Models;
using ReportForm.Common;

namespace Achievement.SearchForManager.Models
{
    public class SeachListModel
    {
        #region プロパティ
        /// <summary>
        /// ユーザーリスト
        /// </summary>
        public List<SearchManagerModel> UserList { get; set; } = null;

        /// <summary>
        /// 参照年月
        /// </summary>
        public string ReportMonth { get; set; } = null;

        /// <summary>
        /// 権限範囲
        /// </summary>
        public SecureLogic.SEARCH_RANGE authority { get; set; } = SecureLogic.SEARCH_RANGE.NONE;

        /// <summary>
        /// 権限
        /// </summary>
        public string IsDisabled(string key, string value, SecureLogic.SEARCH_RANGE range)
        {
            // 固定検索範囲
            if (range == SecureLogic.SEARCH_RANGE.MYSELF)
            {
                // 自分のみの場合はすべてdisabledを返す
                return "disabled";
            }
            if (range == SecureLogic.SEARCH_RANGE.BRANCH && (key == "officeManager" || key == "prefManager" || key == "branchManager"))
            {
                return "disabled";
            }
            if (range == SecureLogic.SEARCH_RANGE.PREF && (key == "officeManager" || key == "prefManager"))
            {
                return "disabled";
            }
            if (range == SecureLogic.SEARCH_RANGE.OFFICE && key == "officeManager")
            {
                return "disabled";
            }
            return "";
        }
        #endregion
    }



}
