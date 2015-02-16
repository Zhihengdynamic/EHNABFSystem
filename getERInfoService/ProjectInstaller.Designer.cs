namespace getERInfoService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.getERInfoserviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.getERInfoserviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // getERInfoserviceProcessInstaller
            // 
            this.getERInfoserviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.getERInfoserviceProcessInstaller.Password = null;
            this.getERInfoserviceProcessInstaller.Username = null;
            // 
            // getERInfoserviceInstaller
            // 
            this.getERInfoserviceInstaller.DisplayName = "getERInfo";
            this.getERInfoserviceInstaller.ServiceName = "getERInfoService1";
            this.getERInfoserviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.getERInfoserviceProcessInstaller,
            this.getERInfoserviceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller getERInfoserviceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller getERInfoserviceInstaller;
    }
}