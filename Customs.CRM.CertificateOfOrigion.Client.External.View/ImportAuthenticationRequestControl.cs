using Customs.CRM.CertificateOfOrigion.Client.External.Api;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Extensions;
using Customs.Inf.MMI.Common.Toolbox.CustomControlBases;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Customs.CRM.CertificateOfOrigion.Client.External.View
{
    public class ImportAuthenticationRequestControl : CustomControlBase
    {

        #region DataMembers

        private IImportAuthenticationRequestExternalPresenter _controlPresenter;
        private bool _isTemplateApplay;

        public IEntity CurrentEntity //lead document id
        {
            get { return (IEntity)GetValue(CurrentEntityProperty); }
            set { SetValue(CurrentEntityProperty, value); }
        }
        /// <summary>
        /// CurrentEntityProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty CurrentEntityProperty =
            DependencyProperty.Register("CurrentEntity", typeof(IEntity), typeof(ImportAuthenticationRequestControl),
                                        new FrameworkPropertyMetadata(null,
                                                                      FrameworkPropertyMetadataOptions.
                                                                          BindsTwoWayByDefault, OnCurrentEntityChanged));

        private static void OnCurrentEntityChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o == null) return;
            var control = o as ImportAuthenticationRequestControl;
            if (control == null || control.CurrentViewModel == null) return;

            if (e.NewValue == null) return;

            control.CurrentViewModel.CurrentEntity = control.CurrentEntity;            

            if (control.CurrentViewModel.GetAuthenticationRequestByLeadDocumentIDCommand == null) return;

            control.CurrentViewModel.GetAuthenticationRequestByLeadDocumentIDCommand.Execute(null);

        }


        public List<int> LeadDocumentIds
        {
            get { return (List<int>)GetValue(LeadDocumentIdsProperty); }
            set { SetValue(LeadDocumentIdsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeadDocumentIds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeadDocumentIdsProperty =
            DependencyProperty.Register("LeadDocumentIds", typeof(List<int>), typeof(ImportAuthenticationRequestControl),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.
                        BindsTwoWayByDefault, OnLeadDocumentIdsChanged));

        private static void OnLeadDocumentIdsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o == null) return;
            var control = o as ImportAuthenticationRequestControl;
            if (control == null || control.CurrentViewModel == null) return;

            if (e.NewValue == null) return;

            control.CurrentViewModel.LeadDocumentIds = control.LeadDocumentIds;

            if (control.CurrentViewModel.GetAuthenticationRequestByLeadDocumentIDCommand == null) return;

            control.CurrentViewModel.GetAuthenticationRequestByLeadDocumentIDCommand.Execute(null);

        }

        public IImportAuthenticationRequestExternalViewModel CurrentViewModel
        {
            get { return (IImportAuthenticationRequestExternalViewModel)GetValue(CurrentViewModelProperty); }
            set { SetValue(CurrentViewModelProperty, value); }
        }
        /// <summary>
        /// CurrentViewModelProperty DependencyProperty
        /// </summary>
        internal static readonly DependencyProperty CurrentViewModelProperty =
            DependencyProperty.Register("CurrentViewModel", typeof(IImportAuthenticationRequestExternalViewModel),
                                        typeof(IImportAuthenticationRequestExternalViewModel));



        public ICommand RefreshCommand
        {
            get { return (ICommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }
        public static readonly DependencyProperty RefreshCommandProperty =
           DependencyProperty.Register("RefreshCommand", typeof(ICommand), typeof(ImportAuthenticationRequestControl));


        #endregion

        #region Ctor

        static ImportAuthenticationRequestControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImportAuthenticationRequestControl),
                                                     new FrameworkPropertyMetadata(typeof(ImportAuthenticationRequestControl)));
        }

        public ImportAuthenticationRequestControl()
        {
            InitControl();
        }

        private void InitControl()
        {
            if (DependencyObjectExtentions.IsDesignMode) return;

            _controlPresenter = CreatePresenterInstance();
            var vm = _controlPresenter.DealFileAuthenticationRequestViewModelEditViewModel;

            // Dependency property
            SetCurrentValue(CurrentViewModelProperty, vm);
            SetCurrentValue(CurrentEntityProperty, vm.CurrentEntity);
            SetCurrentValue(LeadDocumentIdsProperty, vm.LeadDocumentIds);
            SetCurrentValue(RefreshCommandProperty, vm.DealFileImportAuthenticationRequestRefreshCommand);
            

        }
        #endregion


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_isTemplateApplay) return;
            _isTemplateApplay = true;
            

        }



        #region PrivateMethods

        private static IImportAuthenticationRequestExternalPresenter CreatePresenterInstance()
        {
            // if has PresenterType try resolve instance from unity cont.
            IImportAuthenticationRequestExternalPresenter presenterInstance =
                CALFacade.Instance.ClientUnityContainer.Resolve(typeof(IImportAuthenticationRequestExternalPresenter)) as IImportAuthenticationRequestExternalPresenter;
            if (presenterInstance != null) return presenterInstance;

            //  if has PresenterType and cannot resolve from unity cont. try resolve instance by activator
            presenterInstance = Activator.CreateInstance(typeof(IImportAuthenticationRequestExternalPresenter)) as IImportAuthenticationRequestExternalPresenter;

            return presenterInstance;
        }



       


        #endregion

    }
}
