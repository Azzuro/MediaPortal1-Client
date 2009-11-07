﻿using System;
using System.Collections.Generic;
using System.Text;
using MpeCore.Classes.Events;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;

namespace MpeCore.Classes.ActionType
{
    class InstallFiles :IActionType
    {
        public event FileInstalledEventHandler ItemProcessed;
        
        public int ItemsCount(PackageClass packageClass, ActionItem actionItem)
        {
            return packageClass.GetInstallableFileCount();
        }

        public string DisplayName
        {
            get { return "InstallFiles"; }
        }

        public string Description
        {
            get { return "Install all files witch have group checked"; }
        }


        public SectionParamCollection GetDefaultParams()
        {
            return new SectionParamCollection();
        }

        public SectionResponseEnum Execute(PackageClass packageClass, ActionItem actionItem)
        {
            packageClass.FileInstalled += packageClass_FileInstalled;
            packageClass.Install();
            packageClass.FileInstalled -= packageClass_FileInstalled;
            return SectionResponseEnum.Ok;
        }

        public ValidationResponse Validate(PackageClass packageClass, ActionItem actionItem)
        {
            if (!string.IsNullOrEmpty(actionItem.ConditionGroup) && packageClass.Groups[actionItem.ConditionGroup] == null)
                return new ValidationResponse()
                {
                    Message = actionItem.Name + " condition group not found " + actionItem.ConditionGroup,
                    Valid = false
                }; 

            return new ValidationResponse();
        }

        public SectionResponseEnum UnInstall(PackageClass packageClass, UnInstallItem item)
        {
            return SectionResponseEnum.Ok;
        }

        void packageClass_FileInstalled(object sender, InstallEventArgs e)
        {
            if (ItemProcessed != null)
                ItemProcessed(sender, e);
        }


    }
}
