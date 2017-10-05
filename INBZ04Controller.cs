using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NMHI.Models.Domain.COMN;
using NMHI.Models.Domain.INTRA;
using NMHI.Models.Repository.INTRA;
using NMHI.Models.Repository.COMN;
using System.Data;
using System.Collections;
using System.Web.Script.Serialization;

namespace NMHI.Controllers.INTRA
{
    /// <summary>
    /// [인트라넷] 업무관리 > 기안/결재
    /// </summary>
    public class INBZ04Controller : Controller
    {
        private INBZ04Data data = new INBZ04Data();
        /// <summary>
        /// 설  명 : 기안/결재 게시판
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.02
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(INBZ04Ent ent)
        {
            ViewBag.ent = ent;
            return View();
        }

        /// <summary>
        /// 설  명 : 기안/결재 목록
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.03
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public PartialViewResult List(INBZ04Ent ent)
        {
            #region 페이징 파라미터
            if (ent.page == 0) { ent.page = 1; } //현재 페이지
            ent.page_sz = 17; //페이지 사이즈
            ent.tot = ent.tot == 0 ? data.GetApvListCnt(ent) : ent.tot;
            #endregion

            ViewBag.ent = ent;
            ViewBag.dt = data.GetApvList(ent);

            return PartialView();
        }

        /// <summary>
        /// 설  명 : 상세
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.04
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public ActionResult Detail(INBZ04Ent ent)
        {
            ComnData cd = new ComnData();
            cd.Update(ent, "E", "INBZ04.PrcApvView"); //조회기록

            ViewBag.ent = ent;

            DataRow drAuth = data.GetApvAuth(ent);
            DataRow dr = data.GetApvDetail(ent);
            TempData["dr"] = dr;
            ViewBag.dr = drAuth;

            return View();
        }

        /// <summary>
        /// 설  명 : 상세 부분뷰
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.04
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public PartialViewResult Detail_Content(INBZ04Ent ent)
        {
            ViewBag.ent = ent;
            ViewBag.dr = TempData["dr"] == null ? data.GetApvDetail(ent) : TempData["dr"] as DataRow;

            ComnData cd = new ComnData();
            ViewBag.hashRecvInfo = cd.GetRecvInfo(ent.sesHspCd, "GA_APV", ent.no); //수신자
            ViewBag.hashUnReadInfo = cd.GetUnReadInfo(ent.sesHspCd, "GA_APV", ent.no);
            ViewBag.ApvDt = data.GetApvStatInfo(ent.sesHspCd, ent.no);//결재상태

            return PartialView();
        }


        /// <summary>
        /// 설  명 : 기안/결재 상태 조회
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.10
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult ApvStatInfo(INBZ04Ent ent)
        {
            DataTable dt = data.GetApvStatInfo(ent.sesHspCd, ent.hdnKeyNo);
            
            return Json(JsonFormat(dt));
        }

        /// <summary>
        /// 설  명 : 작성
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.03
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public ActionResult Input(INBZ04Ent ent)
        {
            ViewBag.ent = ent;

            ComnData cd = new ComnData();
            if (ent.no != 0)
            {
                ViewBag.dr = data.GetApvDetail(ent);
                ViewBag.hashRecvInfo = cd.GetRecvInfo(ent.sesHspCd, "GA_APV", ent.no);//수신자
            }

            return View();
        }

        /// <summary>
        /// 설  명 : 입력/수정 처리
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.04
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult InputBiz(INBZ04Ent ent)
        {
            #region 도메인 항목 [AllowHtml] 설정 시 ModelBinder 기능 동작하지 않아서(확인 필요) 세션 정보 넣어주는 로직 해당 Action에서 다시 정의
            UserInfo user = (UserInfo)Session["UserInfo"];
            ent.sesHspCd = user.sesHspCd;
            ent.sesLoginEmpNo = user.sesLoginEmpNo;
            ent.sesEmpNm = user.sesEmpNm;
            ent.sesEmail = user.sesEmail;
            #endregion

            ComnData cd = new ComnData();
            RsltEnt rslt;

            string strTblNm = "GA_APV", strMode;
            ent.userIP = Request.UserHostAddress;

            ent.txtStrtDt = ent.txtStrtDt == null ? null : ent.txtStrtDt.Replace("-", "");
            ent.txtEndDt = ent.txtEndDt == null ? null : ent.txtEndDt.Replace("-", "");

            if (ent.hdnKeyNo == 0)
            {
                //입력
                strMode = "I";
                ent.hdnKeyNo = data.GetMaxKeyNo(ent.sesHspCd);
                ent.pPrdYr = DateTime.Now.ToString("yyyy");
                ent.hdnDocNo = data.GetMaxDocNo(ent);
                ent.pPrdYr = ""; //초기화
                rslt = cd.Update(ent, strMode, "INBZ04.InsApv");
            }
            else
            {
                //수정
                strMode = "U";
                rslt = cd.Update(ent, strMode, "INBZ04.UpdApv");
            }

            #region 결재자
            string[] arrApvEmpInfo;
            if (!string.IsNullOrWhiteSpace(ent.hdnApvEmpInfo))
            {
                arrApvEmpInfo = ent.hdnApvEmpInfo.Split(',');
                data.UpsertApvLineInfo(ent.sesHspCd, ent.hdnKeyNo, arrApvEmpInfo, ent.sesLoginEmpNo, ent.userIP);
            }
            else
            {
                arrApvEmpInfo = null;
            }
            #endregion

            #region 수신자
            string[] arrRecvInfo;
            if (!string.IsNullOrWhiteSpace(ent.hdnRecvInfo))
            {
                ent.rdoRecvInfoYn = "Y";
                arrRecvInfo = ent.hdnRecvInfo.Split(',');
                cd.UpsertRecvInfo(ent.sesHspCd, strTblNm, ent.hdnKeyNo, arrRecvInfo, ent.sesLoginEmpNo, ent.userIP);
            }
            else
            {
                ent.rdoRecvInfoYn = "N";
                arrRecvInfo = null;
            }
            #endregion

            #region 첨부파일
            HttpFileCollectionBase files = Request.Files;
            if (files.Count > 0)
            {
                FileAtchData fad = new FileAtchData();
                fad.InsertFileAtch(ent.sesHspCd, strTblNm, ent.hdnKeyNo, files, ent.sesLoginEmpNo, ent.userIP);
            }
            #endregion

            return Json(rslt, "text/json");
        }

        /// <summary>
        /// 설  명 : 기안/결재 삭제
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.05
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult Delete(INBZ04Ent ent)
        {
            FileAtchData fad = new FileAtchData();
            DataTable dt = fad.GetFileAtchList(ent.sesHspCd, "GA_APV", ent.hdnKeyNo);
            string strFilePath;
            foreach (DataRow row in dt.Rows)
            {
                strFilePath = row["FILE_PATH"].ToString();
                if (System.IO.File.Exists(strFilePath))
                {
                    System.IO.File.Delete(strFilePath);
                }
            }

            ComnData cd = new ComnData();
            RsltEnt rslt = cd.Update(ent, "D", "INBZ04.DelApv");
            return Json(rslt, "text/json");
        }

        /// <summary>
        /// 설  명 : 결재자 조회
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.10
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult ApvEmpInfo(INBZ04Ent ent)
        {
            #region 도메인 항목 [AllowHtml] 설정 시 ModelBinder 기능 동작하지 않아서(확인 필요) 세션 정보 넣어주는 로직 해당 Action에서 다시 정의
            UserInfo user = (UserInfo)Session["UserInfo"];
            ent.sesHspCd = user.sesHspCd;
            ent.sesLoginEmpNo = user.sesLoginEmpNo;
            ent.sesEmpNm = user.sesEmpNm;
            ent.sesEmail = user.sesEmail;
            #endregion

            ComnData cd = new ComnData();

            Hashtable ht = data.GetApvLineInfo(ent.sesHspCd, ent.hdnKeyNo, ent.sesLoginEmpNo, ent.rdoApvDocType); //결재자
                        
            return Json(ht); 
        }

        /// <summary>
        /// 설  명 : DataTable => Json 
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.11
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> JsonFormat(DataTable dt)
        {   
            //JavaScriptSerializer serializer = new JavaScriptSerializer();

            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }
            }

            return rows;// serializer.Serialize(rows);
        }

        /// <summary>
        /// 설  명 : 기안/결재 Form 조회
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.10
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult ApvFormList(INBZ04Ent ent)
        {
            #region 도메인 항목 [AllowHtml] 설정 시 ModelBinder 기능 동작하지 않아서(확인 필요) 세션 정보 넣어주는 로직 해당 Action에서 다시 정의
            UserInfo user = (UserInfo)Session["UserInfo"];
            ent.sesHspCd = user.sesHspCd;
            ent.sesLoginEmpNo = user.sesLoginEmpNo;
            ent.sesEmpNm = user.sesEmpNm;
            ent.sesEmail = user.sesEmail;
            #endregion

            DataTable dt = data.GetApvFormList(ent);
            
            return Json(JsonFormat(dt));
        }

        /// <summary>
        /// 설  명 : 기안 템플릿 저장 처리
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.10
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent">
        /// </param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult FormInputBiz(INBZ04Ent ent)
        {
            #region 도메인 항목 [AllowHtml] 설정 시 ModelBinder 기능 동작하지 않아서(확인 필요) 세션 정보 넣어주는 로직 해당 Action에서 다시 정의
            UserInfo user = (UserInfo)Session["UserInfo"];
            ent.sesHspCd = user.sesHspCd;
            ent.sesLoginEmpNo = user.sesLoginEmpNo;
            ent.sesEmpNm = user.sesEmpNm;
            ent.sesEmail = user.sesEmail;
            #endregion

            ent.userIP = Request.UserHostAddress;
            //공유 여부
            if (string.IsNullOrEmpty(ent.hdnFormShareYn))
            {
                ent.hdnFormShareYn = "N";
            }

            ComnData cd = new ComnData();
            string strMode = ent.hdnFormMode;
            string strSqlNm;
            switch (strMode)
            {
                case "U":
                    //템플릿은 공유여부만 수정가능
                    strSqlNm = "INBZ04.UpdApvForm";
                    break;
                case "D":
                    strSqlNm = "INBZ04.DelApvForm";
                    break;
                default:
                    ent.hdnFormNo = data.GetFormMaxKeyNo(ent.sesHspCd);
                    strSqlNm = "INBZ04.InsApvForm";
                    break;
            }
            RsltEnt rslt = cd.Update(ent, strMode, strSqlNm);

            return Json(rslt, "text/json");
        }

        /// <summary>
        /// 설  명 : 승인/반려
        /// 작성자 : 김성희
        /// 작성일 : 2015.03.04
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult ApvRtnBiz(INBZ04Ent ent)
        {
            ent.userIP = Request.UserHostAddress;    
            
            ComnData cd = new ComnData();
            RsltEnt rslt = cd.Update(ent, ent.hdnApvStatCd, "INBZ04.UptApvRtn");


            string strMode;
            switch (ent.hdnApvStatCd)
            {
                case "A":
                    strMode = "승인";
                    break;
                case "R":
                    strMode = "반려";
                    break;
                case "C":
                    strMode = "승인취소";
                    break;
                default:
                    strMode = "저장";
                    break;
            }
            if (rslt.bRslt)
            {
                rslt.strMsg = string.Format("{0}하였습니다.", strMode);
            }
            else
            {
                rslt.strMsg = string.Format("{0}에 실패하였습니다.\n잠시후 다시 시도해 주십시오.", strMode);
            }
            
            return Json(rslt, "text/json");
        }

        /// <summary>
        /// 설  명 : 집행
        /// 작성자 : 김성희
        /// 작성일 : 2016.02.11
        /// 
        /// 내  용 : 
        /// 수정자 : 
        /// 수정일 : 
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public JsonResult ExecInputBiz(INBZ04Ent ent)
        {
            ent.txtExecDt = string.IsNullOrEmpty(ent.txtExecDt) ? null : ent.txtExecDt.Replace("-", "");
            ComnData cd = new ComnData();
            RsltEnt rslt = cd.Update(ent, "U" , "INBZ04.UptExec");

            return Json(rslt, "text/json");
        }
    }
}
