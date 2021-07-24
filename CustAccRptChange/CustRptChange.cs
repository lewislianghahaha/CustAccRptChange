using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.AR.App.Report;

namespace CustAccRptChange
{
    public class CustRptChange : AgingAnalysisService
    {
        //定义临时表数组
        private string[] _customRptTempDt;

        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            //创建临时表,用于存放自已的数据
            var dbservice = ServiceHelper.GetService<IDBService>();
            _customRptTempDt = dbservice.CreateTemporaryTableName(Context, 1);
            var strDt = _customRptTempDt[0];

            //调用基类的方法,获取初步的查询结果赋值到临时表
            base.BuilderReportSqlAndTempTable(filter, strDt);

            //对初步的查询结果进行处理,然后写回基类默认的存放查询结果的临时表
            var strSql = $@"
                               SELECT T1.*,CAST(T3.FCREDITAMOUNT AS NUMERIC(10,2)) FCREDIT
                               INTO {tableName}
                               FROM {strDt} T1
                               INNER JOIN T_BD_CUSTOMER T2 ON T1.FCONTACTUNITNUMBER=T2.FNUMBER
                               LEFT JOIN T_CRE_CUSTARCHIVESENTRY T3 ON T2.FCUSTID=T3.FOBJECTID
                          ";
            DBUtils.Execute(Context, strSql);
        }

        /// <summary>
        /// 重写报表动态列
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            var reportHeader = base.GetReportHeaders(filter);
            reportHeader.AddChild("FCREDIT", new LocaleValue("信用额度", this.Context.LogLocale.LCID));
            return reportHeader;
        }

        /// <summary>
        /// 关闭报表时执行
        /// </summary>
        public override void CloseReport()
        {
            //删除临时表
            if (_customRptTempDt.IsNullOrEmptyOrWhiteSpace())
            {
                return;
            }
            var dbService = ServiceHelper.GetService<IDBService>();
            //使用后的临时表删除
            dbService.DeleteTemporaryTableName(Context, _customRptTempDt);

            base.CloseReport();
        }
    }
}
