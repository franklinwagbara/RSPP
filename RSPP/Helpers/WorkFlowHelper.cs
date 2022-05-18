using log4net;
using Microsoft.AspNetCore.Mvc;
using RSPP.Helpers;
using RSPP.Models;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RSPP.Helper
{
    public class WorkFlowHelper : Controller
    {
        public RSPPdbContext _context;
        GeneralClass generalClass = new GeneralClass(); 
        private static ILog logger = log4net.LogManager.GetLogger(typeof(WorkFlowHelper));
        public WorkFlowHelper(RSPPdbContext context) {
            _context = context;

        }

        public ResponseWrapper processAction(string ApplicationId, string Action, string FromUserId, string Comment)
        {

            UserMaster nextUser = null;
            string NextProcessor = null;
            string applicationLocation = null;
            ResponseWrapper responsewrapper = new ResponseWrapper();
            List<string> applicationTypeList = new List<string>();
            WorkFlowNavigation mainWkflowNavigation = default(WorkFlowNavigation);
            try
            {
                //var statecode = (from s in _context.ApplicationRequestForm where s.ApplicationId == ApplicationId select s.StateCode).FirstOrDefault();
                logger.Info("UserID =>" + FromUserId + ", Action => " + Action + ", ApplicationID =>" + ApplicationId);

                applicationTypeList.Add("ALL");
                var userMaster = (from u in _context.UserMaster where u.UserEmail == FromUserId && u.Status == "ACTIVE" select u).FirstOrDefault();
                //UserMaster userMaster = _context.UserMaster.Where(c => c.UserEmail.Trim() == FromUserId.Trim() && c.Status == "ACTIVE").FirstOrDefault();


                if (userMaster ==
                 default(UserMaster))
                {
                    responsewrapper.status = false;
                    responsewrapper.value = "USER RECORD WITH ID " + FromUserId + " CANNOT BE FOUND ON THE SYSTEM";
                    return responsewrapper;
                }

                logger.Info("Done with UserMaster Record");
                ApplicationRequestForm appmaster = _context.ApplicationRequestForm.Where(c => c.ApplicationId == ApplicationId).FirstOrDefault();
               // var fld = appmaster.IsLegacy == "YES" ? (from u in dbCtxt.ApplicationRequests join w in dbCtxt.WorkFlowNavigations on u.CurrentStageID equals w.CurrentStageID where u.IsLegacy == "YES" && u.ApplicationId == ApplicationId && w.LicenseTypeId == Appcode select new { w.FieldLocationApply }).FirstOrDefault() : (from u in dbCtxt.ApplicationRequests join w in dbCtxt.WorkFlowNavigations on u.CurrentStageID equals w.CurrentStageID where u.LicenseReference == null && u.ApplicationId == ApplicationId && w.LicenseTypeId == Appcode select new { w.FieldLocationApply }).ToList().LastOrDefault();

                if (appmaster == default(ApplicationRequestForm))

                {
                    responsewrapper.status = false;
                    responsewrapper.value = "APPLICATION REFERENCE " + ApplicationId + " CANNOT BE FOUND ON THE SYSTEM";
                    return responsewrapper;
                }

                logger.Info("Done with Application Record");
                //applicationTypeList.Add(appmaster.ApplicationTypeId.Trim());
                // List<WorkFlowNavigation> wkflowNavigationList = dbCtxt.WorkFlowNavigations.Where(c => c.CurrentStageID == appmaster.CurrentStageID && (appmaster.ApplicationID == ApplicationId) && (appmaster.LastAssignedUser == FromUserId) && (c.Action == Action) && (c.LicenseTypeCode == Appcode)).ToList();//&& applicationTypeList.Contains(c.ApplicationType.Trim())
                var wkflowNavigationList = (from w in _context.WorkFlowNavigation
                                            join a in _context.ApplicationRequestForm on w.CurrentStageId
                                            equals a.CurrentStageId
                                            where a.ApplicationId == ApplicationId && a.LastAssignedUser == FromUserId && w.Action == Action
                                            select w).ToList();
                if (wkflowNavigationList.Count == 0)
                {
                    responsewrapper.status = false;
                    responsewrapper.value = "WORKFLOW NAVIGATION PARAMETER => " + appmaster.ApplicationId + "," + appmaster.CurrentStageId + "," + Action + " CANNOT BE RETRIEVED, CONTACT ADMINISTRATOR";
                    return responsewrapper;
                }
                else
                {
                    foreach (var wkflowNavigation in wkflowNavigationList)
                    {
                        //EXAMPLE  2  =  2
                        
                            mainWkflowNavigation = wkflowNavigation;
                            break;
                    }

                        //EXAMPLE  2  !=  4
                        
                    
                }
                responsewrapper.receivedLocation = "";
                logger.Info("Done with WorkFlow Navigation Record");
                logger.Info("Current User Role =>" + userMaster.UserRole);
                //logger.Info("Target Role =>" + mainWkflowNavigation.TargetRole);
                logger.Info("Company UserId =>" + appmaster.CompanyEmail);
                
                if (mainWkflowNavigation.TargetRole == "COMPANY")
                {
                    NextProcessor = appmaster.CompanyEmail;
                    nextUser = _context.UserMaster.Where(u => u.UserEmail == appmaster.CompanyEmail).FirstOrDefault();
                    appmaster.CurrentStageId = mainWkflowNavigation.NextStateId;
                    if (Action == "Accept" && mainWkflowNavigation.ActionRole == "REGISTRAR")
                    {
                        appmaster.Status = "Approved";
                        var subject = "Application Approved";
                        string content = "Application with the reference number " + ApplicationId + " has been approved successfully.";
                        var sendemail = generalClass.SendStaffEmailMessage(NextProcessor, subject, content);
                    }
                    else if(Action == "Reject")
                    {
                        appmaster.Status = "Rejected";
                    }
                }
                else
                {

                    var targettorole = wkflowNavigationList.FirstOrDefault().TargetRole;

                   // var applocations = (from a in dbCtxt.ActionHistories where a.ApplicationId == ApplicationId && a.TargetedToRole == targettorole select a.CurrentFieldLocation).ToList().LastOrDefault();
                    var nxtusers = (from a in _context.ActionHistory where a.ApplicationId == ApplicationId && a.TriggeredByRole == targettorole select new { a.TriggeredBy }).ToList().LastOrDefault();


                    if (appmaster.CurrentStageId == wkflowNavigationList.FirstOrDefault().CurrentStageId)//14
                    {
                        //var location = (from l in dbCtxt.FieldLocations where l.FieldLocationID == appmaster.CurrentOfficeLocation select l.FieldLocationID).FirstOrDefault();//l.FieldType == mainWkflowNavigation.FieldLocationApply && 
                        appmaster.Status = "Processing";
                        var histry = (from a in _context.ActionHistory where a.ApplicationId == ApplicationId && a.Action == "Reject" select new { a.TriggeredBy, a.CurrentStageId }).ToList().LastOrDefault();

                        if (mainWkflowNavigation.ActionRole == "COMPANY" && Action == "ReSubmit")
                        {
                            appmaster.Status = "Processing";
                            appmaster.CurrentStageId= histry.CurrentStageId;
                            NextProcessor = histry.TriggeredBy;
                            //applicationLocation = appmaster.CurrentOfficeLocation;
                        }


                       

                        else if (Action == "Reject")
                        {

                            
                                //var applocation = (from a in dbCtxt.ActionHistories where a.ApplicationId == ApplicationId && a.TargetedToRole == targettorole select a.CurrentFieldLocation).ToList().LastOrDefault();
                                var nxtuser = (from a in _context.ActionHistory where a.ApplicationId == ApplicationId && a.TriggeredByRole == targettorole select new { a.TriggeredBy }).ToList().LastOrDefault();

                               // applicationLocation = applocation;
                                if (nxtuser != null) { NextProcessor = nxtuser.TriggeredBy; }
                                appmaster.CurrentStageId = wkflowNavigationList.FirstOrDefault().NextStateId;
                           

                        }
                       
                        else
                        {
                            nextUser = _context.UserMaster.Where(u => u.UserRole.Contains(mainWkflowNavigation.TargetRole) && u.Status == "ACTIVE").FirstOrDefault();
                            NextProcessor = generalClass.GetNextProcessingStaff(_context, appmaster, mainWkflowNavigation.TargetRole, applicationLocation, Action, mainWkflowNavigation.ActionRole);
                            appmaster.CurrentStageId = mainWkflowNavigation.NextStateId;

                            
                        }
                        
                    }
                }



                if (!string.IsNullOrEmpty(NextProcessor))
                {
                    nextUser = _context.UserMaster.Where(u => u.UserEmail == NextProcessor).FirstOrDefault();
                }

                else
                {
                    responsewrapper.status = false;
                    responsewrapper.value = "No User was maintaned for the Next WorkFlow on the System, Kindly Liase with Admin and try again";
                    return responsewrapper;
                }

                appmaster.LastAssignedUser = NextProcessor;

                //appmaster.CurrentOfficeLocation = applicationLocation;
                appmaster.ModifiedDate = DateTime.Now;
                logger.Info("About to Start Notification");
                if (mainWkflowNavigation.TargetRole != null)
                {
                    foreach (string targetRole in mainWkflowNavigation.TargetRole.Split(','))
                    {
                        insertIntoHistory(appmaster, userMaster, nextUser, Action, Comment, mainWkflowNavigation.CurrentStageId, mainWkflowNavigation.NextStateId);
                    }
                }

                responsewrapper.status = true;

                string stateType = null;
                String nextStageName = null;
                WorkFlowState wkflowstate = _context.WorkFlowState.Where(w => w.StateId == mainWkflowNavigation.NextStateId).FirstOrDefault();
                stateType = wkflowstate.StateType;
                nextStageName = wkflowstate.StateName;
                responsewrapper.receivedBy = appmaster.LastAssignedUser;
                responsewrapper.nextStageId = Convert.ToString(mainWkflowNavigation.NextStateId);
                responsewrapper.receivedLocation = "";
                logger.Info("Done With Notification");
                _context.SaveChanges();
                if (mainWkflowNavigation.TargetRole != "COMPANY")
                {

                    var subject = "Application waiting for Approval";
                    var content = "Application with the reference number " + ApplicationId + " is currently on your desk waiting for approval.";

                    var outofofficestaff = (from a in _context.OutofOffice where a.Relieved == appmaster.LastAssignedUser && a.Status == "Started" select a).FirstOrDefault();
                    if (outofofficestaff != null)
                    {
                        var subject1 = "Relieved Application waiting for Approval";
                        var content1 = "Application is currently on " + outofofficestaff.Relieved + " desk waiting for your approval. NOTE: You relieved " + outofofficestaff.Relieved;

                        generalClass.SendStaffEmailMessage(outofofficestaff.Reliever, subject1, content1);
                    }

                    var sendmail = generalClass.SendStaffEmailMessage(NextProcessor, subject, content);

                   

                }


                var received = (from w in _context.ActionHistory where w.ApplicationId == appmaster.ApplicationId select w).ToList().LastOrDefault();
                responsewrapper.receivedByRole = received.TargetedToRole;
                
                logger.Info("Current State Type =>" + stateType);

                if (stateType == "PROGRESS")
                {
                    responsewrapper.value = "Application has been moved To " + responsewrapper.receivedBy + "(" + responsewrapper.receivedByRole + ")"; 
                }
                else if (stateType == "COMPLETE")
                {
                    responsewrapper.value = "Application Has been Approved and Certificate Generated ";
                }
                else if (stateType == "LCOMPLETE")
                {
                    responsewrapper.value = "Legacy License Approved";
                }
                else if (stateType == "REJECTED")
                {
                    responsewrapper.value = "Approval Has been Denied and License/Approval Rejected ";
                }
                else if (stateType == "CONTINUE" || stateType == "START")
                {
                    responsewrapper.value = "Application Has been Moved To " + appmaster.CompanyEmail+" ("+ appmaster.AgencyName +")"+ " (" + nextStageName.ToUpper() + ") Stage";
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex.InnerException);

                responsewrapper.status = false;
                responsewrapper.value = "General Error occured during Workflow Navigation, Please Contact Support";
                return responsewrapper;
            }

            return responsewrapper;
        }


        private void insertIntoHistory(ApplicationRequestForm appRequest, UserMaster actionUserMaster, UserMaster targetUserMaster, string userAction, string comment, int currentStageId, int nextStageId)
        {
            ActionHistory actionHistory = new ActionHistory();
            actionHistory.ApplicationId = appRequest.ApplicationId;
            actionHistory.CurrentStageId = (short)currentStageId;
            actionHistory.Action = userAction;
            actionHistory.ActionDate = DateTime.UtcNow;
            actionHistory.Message = comment;
            actionHistory.TriggeredBy = actionUserMaster.UserEmail;
            actionHistory.TriggeredByRole = actionUserMaster.UserRole;

            if (targetUserMaster != default(UserMaster))
            {
                actionHistory.TargetedTo = targetUserMaster.UserEmail;
                actionHistory.TargetedToRole = targetUserMaster.UserRole;
            }
            actionHistory.NextStateId = (short)nextStageId;
            _context.ActionHistory.Add(actionHistory);
        }


    }
}