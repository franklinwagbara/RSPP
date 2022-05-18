using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RSPP.Models.DB
{
    public partial class RSPPdbContext : DbContext
    {
        public RSPPdbContext()
        {
        }

        public RSPPdbContext(DbContextOptions<RSPPdbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ActionHistory> ActionHistory { get; set; }
        public virtual DbSet<Agency> Agency { get; set; }
        public virtual DbSet<ApplicationRequestForm> ApplicationRequestForm { get; set; }
        public virtual DbSet<CertificateSerialNumber> CertificateSerialNumber { get; set; }
        public virtual DbSet<Configuration> Configuration { get; set; }
        public virtual DbSet<Documents> Documents { get; set; }
        public virtual DbSet<ExtraPayment> ExtraPayment { get; set; }
        public virtual DbSet<GovernmentAgency> GovernmentAgency { get; set; }
        public virtual DbSet<LineOfBusiness> LineOfBusiness { get; set; }
        public virtual DbSet<LogisticsServiceProvider> LogisticsServiceProvider { get; set; }
        public virtual DbSet<MissingDocuments> MissingDocuments { get; set; }
        public virtual DbSet<OtherPortServiceProvider> OtherPortServiceProvider { get; set; }
        public virtual DbSet<OutofOffice> OutofOffice { get; set; }
        public virtual DbSet<PaymentCategory> PaymentCategory { get; set; }
        public virtual DbSet<PaymentLog> PaymentLog { get; set; }
        public virtual DbSet<PortOffDockTerminalOperator> PortOffDockTerminalOperator { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<ShippingAgency> ShippingAgency { get; set; }
        public virtual DbSet<UploadedDocuments> UploadedDocuments { get; set; }
        public virtual DbSet<UserLogin> UserLogin { get; set; }
        public virtual DbSet<UserMaster> UserMaster { get; set; }
        public virtual DbSet<UserOfPortService> UserOfPortService { get; set; }
        public virtual DbSet<WorkFlowNavigation> WorkFlowNavigation { get; set; }
        public virtual DbSet<WorkFlowState> WorkFlowState { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlServer("Server=LAPTOP-N5F7SSUF\\SQLEXPRESS;Database=RSPPdb;Trusted_Connection=True;");
                optionsBuilder.UseSqlServer("Server=tcp:staging-servers.database.windows.net,1433;Initial Catalog=RPRSPU_DB;Persist Security Info=False;User ID=serveradmin;Password=*123*brandonetech#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False");

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActionHistory>(entity =>
            {
                entity.HasKey(e => e.ActionId);

                entity.Property(e => e.Action)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ActionDate).HasColumnType("datetime");

                entity.Property(e => e.ApplicationId)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CurrentStageId).HasColumnName("CurrentStageID");

                entity.Property(e => e.Message)
                    .HasColumnName("MESSAGE")
                    .HasColumnType("text");

                entity.Property(e => e.NextStateId).HasColumnName("NextStateID");

                entity.Property(e => e.TargetedTo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TargetedToRole)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.TriggeredBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TriggeredByRole)
                    .HasMaxLength(150)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Agency>(entity =>
            {
                entity.Property(e => e.AgencyName)
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ApplicationRequestForm>(entity =>
            {
                entity.HasKey(e => e.ApplicationId)
                    .HasName("PK_ApplicationRequestForm_1");

                entity.Property(e => e.ApplicationId)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.AddedDate).HasColumnType("datetime");

                entity.Property(e => e.AgencyName)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ApplicationTypeId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CacregNum)
                    .HasColumnName("CACRegNum")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyAddress).IsUnicode(false);

                entity.Property(e => e.CompanyEmail)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyWebsite)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CurrentStageId).HasColumnName("CurrentStageID");

                entity.Property(e => e.DateofEstablishment).HasColumnType("datetime");

                entity.Property(e => e.IsLegacy)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LastAssignedUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LicenseExpiryDate).HasColumnType("datetime");

                entity.Property(e => e.LicenseIssuedDate).HasColumnType("datetime");

                entity.Property(e => e.LicenseReference)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.NameOfAssociation)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNum)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.PostalAddress).IsUnicode(false);

                entity.Property(e => e.PrintedStatus)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.HasOne(d => d.Agency)
                    .WithMany(p => p.ApplicationRequestForm)
                    .HasForeignKey(d => d.AgencyId)
                    .HasConstraintName("FK_ApplicationRequestForm_Agency");
            });

            modelBuilder.Entity<Configuration>(entity =>
            {
                entity.HasKey(e => e.ParamId);

                entity.Property(e => e.ParamId)
                    .HasColumnName("ParamID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ParamValue)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Documents>(entity =>
            {
                entity.HasKey(e => e.DocId);

                entity.Property(e => e.DocumentName)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.IsMandatory)
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.HasOne(d => d.LineOfBusiness)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.LineOfBusinessId)
                    .HasConstraintName("FK_Documents_Agency");
            });

            modelBuilder.Entity<ExtraPayment>(entity =>
            {
                entity.Property(e => e.ExtraPaymentId).HasColumnName("ExtraPaymentID");

                entity.Property(e => e.Account)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.AppReceiptId)
                    .HasColumnName("AppReceiptID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ApplicantId)
                    .IsRequired()
                    .HasColumnName("ApplicantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApplicationId)
                    .IsRequired()
                    .HasColumnName("ApplicationID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Arrears).HasColumnType("decimal(20, 2)");

                entity.Property(e => e.BankCode)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.ExtraPaymentAppRef)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ExtraPaymentBy)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.LastRetryDate).HasColumnType("datetime");

                entity.Property(e => e.LicenseTypeCode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Rrreference)
                    .HasColumnName("RRReference")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SanctionType)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionDate).HasColumnType("datetime");

                entity.Property(e => e.TransactionId)
                    .HasColumnName("TransactionID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.TxnAmount).HasColumnType("decimal(20, 2)");

                entity.Property(e => e.TxnMessage)
                    .HasMaxLength(150)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<GovernmentAgency>(entity =>
            {
                entity.HasKey(e => e.GovAgencyId);

                entity.Property(e => e.AnyOtherRelevantInfo)
                    .HasColumnName("Any_Other_Relevant_Info")
                    .IsUnicode(false);

                entity.Property(e => e.ApplicationId)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ServicesProvidedInPort)
                    .HasColumnName("Services_Provided_In_Port")
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LineOfBusiness>(entity =>
            {
                entity.HasKey(e => e.OrderId);

                entity.Property(e => e.OrderId).ValueGeneratedNever();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.LineOfBusinessName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LogisticsServiceProvider>(entity =>
            {
                entity.ToTable("Logistics_Service_Provider");

                entity.Property(e => e.LogisticsServiceProviderId).HasColumnName("Logistics_Service_Provider_Id");

                entity.Property(e => e.AnyOtherInfo)
                    .HasColumnName("Any_Other_Info")
                    .IsUnicode(false);

                entity.Property(e => e.ApplicationId)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CrffnRegistrationNum)
                    .HasColumnName("CRFFN_Registration_Num")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CrffnRegistratonExpiryDate)
                    .HasColumnName("CRFFN_Registraton_ExpiryDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.CustomLicenseExpiryDate)
                    .HasColumnName("Custom_License_ExpiryDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.CustomLicenseNum)
                    .HasColumnName("Custom_License_Num")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LineOfBusiness)
                    .HasColumnName("Line_of_Business")
                    .IsUnicode(false);

                entity.Property(e => e.OtherLicense)
                    .HasColumnName("Other_License")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.OtherLicenseExpiryDate)
                    .HasColumnName("Other_License_ExpiryDate")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<MissingDocuments>(entity =>
            {
                entity.HasKey(e => e.MissingDocId);

                entity.Property(e => e.ApplicationId)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Application)
                    .WithMany(p => p.MissingDocuments)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MissingDocuments_ApplicationRequestForm");
            });

            modelBuilder.Entity<OtherPortServiceProvider>(entity =>
            {
                entity.ToTable("Other_Port_Service_Provider");

                entity.Property(e => e.OtherPortServiceProviderId).HasColumnName("Other_Port_Service_Provider_Id");

                entity.Property(e => e.AnyOtherInfo)
                    .HasColumnName("Any_Other_Info")
                    .IsUnicode(false);

                entity.Property(e => e.ApplicationId)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LineOfBusiness)
                    .HasColumnName("Line_of_Business")
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OutofOffice>(entity =>
            {
                entity.Property(e => e.Comment).IsUnicode(false);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Relieved)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Reliever)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(15)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PaymentCategory>(entity =>
            {
                entity.Property(e => e.PaymentAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PaymentCategoryName)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PaymentLog>(entity =>
            {
                entity.HasKey(e => e.PaymentId);

                entity.Property(e => e.PaymentId).HasColumnName("PaymentID");

                entity.Property(e => e.Account)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ActionBy)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.AppReceiptId)
                    .HasColumnName("AppReceiptID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ApplicantId)
                    .IsRequired()
                    .HasColumnName("ApplicantID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApplicationId)
                    .IsRequired()
                    .HasColumnName("ApplicationID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Arrears).HasColumnType("decimal(20, 2)");

                entity.Property(e => e.BankCode)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.LastRetryDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentCategory).IsUnicode(false);

                entity.Property(e => e.Rrreference)
                    .HasColumnName("RRReference")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionDate).HasColumnType("datetime");

                entity.Property(e => e.TransactionId)
                    .HasColumnName("TransactionID")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.TxnAmount).HasColumnType("decimal(20, 2)");

                entity.Property(e => e.TxnMessage)
                    .HasMaxLength(150)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PortOffDockTerminalOperator>(entity =>
            {
                entity.HasKey(e => e.PortOffTerminalOperatorId);

                entity.ToTable("Port_Off_Dock_Terminal_Operator");

                entity.Property(e => e.PortOffTerminalOperatorId).HasColumnName("Port_Off_Terminal_OperatorId");

                entity.Property(e => e.AnyOtherInfo)
                    .HasColumnName("Any_Other_Info")
                    .IsUnicode(false);

                entity.Property(e => e.ApplicationId)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CargoType)
                    .HasColumnName("Cargo_Type")
                    .IsUnicode(false);

                entity.Property(e => e.LineOfBusiness)
                    .HasColumnName("Line_of_Business")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LocationOfTerminal)
                    .HasColumnName("Location_of_Terminal")
                    .IsUnicode(false);

                entity.Property(e => e.NameOfTerminal)
                    .HasColumnName("Name_of_Terminal")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.StatusOfTerminal)
                    .HasColumnName("Status_of_Terminal")
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleId)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ShippingAgency>(entity =>
            {
                entity.ToTable("Shipping_Agency");

                entity.Property(e => e.ShippingAgencyId).HasColumnName("Shipping_Agency_Id");

                entity.Property(e => e.AnyOtherInfo)
                    .HasColumnName("Any_Other_Info")
                    .IsUnicode(false);

                entity.Property(e => e.ApplicationId)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CargoType)
                    .HasColumnName("Cargo_Type")
                    .IsUnicode(false);

                entity.Property(e => e.LineOfBusiness)
                    .HasColumnName("Line_of_Business")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Nimasaregnum)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Nparegnum)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.VesselLinesRepresentedInNigeria)
                    .HasColumnName("Vessel_Lines_Represented_in_Nigeria")
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UploadedDocuments>(entity =>
            {
                entity.HasKey(e => e.DocumentUploadId);

                entity.Property(e => e.ApplicationId)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DocumentName).IsUnicode(false);

                entity.Property(e => e.DocumentSource).IsUnicode(false);
            });

            modelBuilder.Entity<UserLogin>(entity =>
            {
                entity.HasKey(e => new { e.LoginPk, e.UserEmail });

                entity.Property(e => e.LoginPk).ValueGeneratedOnAdd();

                entity.Property(e => e.UserEmail)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Browser).IsUnicode(false);

                entity.Property(e => e.Client)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LoginMessage).IsUnicode(false);

                entity.Property(e => e.LoginTime).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UserType)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserMaster>(entity =>
            {
                entity.Property(e => e.CompanyAddress).IsUnicode(false);

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(350)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LastComment).IsUnicode(false);

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.Property(e => e.LastName)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNum)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.SignatureImage)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.UserEmail)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.UserRole)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserType)
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserOfPortService>(entity =>
            {
                entity.Property(e => e.AnyOtherInfo).IsUnicode(false);

                entity.Property(e => e.ApplicationId)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Category)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.NepcRegNo)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<WorkFlowNavigation>(entity =>
            {
                entity.HasKey(e => e.WorkFlowId);

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ActionRole)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CurrentStageId).HasColumnName("CurrentStageID");

                entity.Property(e => e.NextStateId).HasColumnName("NextStateID");

                entity.Property(e => e.TargetRole)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<WorkFlowState>(entity =>
            {
                entity.HasKey(e => e.StateId);

                entity.Property(e => e.StateId)
                    .HasColumnName("StateID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Progress)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.StateName)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.StateType)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
