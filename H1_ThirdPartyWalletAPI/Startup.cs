using Coravel;
using Coravel.Scheduling.Schedule;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.HealthCheck;
using H1_ThirdPartyWalletAPI.Middleware;
using H1_ThirdPartyWalletAPI.Middlewares.JDB;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.AE;
using H1_ThirdPartyWalletAPI.Service.Game.BTI;
using H1_ThirdPartyWalletAPI.Service.Game.CMD368;
using H1_ThirdPartyWalletAPI.Service.Game.CR;
using H1_ThirdPartyWalletAPI.Service.Game.DS;
using H1_ThirdPartyWalletAPI.Service.Game.EGSlot;
using H1_ThirdPartyWalletAPI.Service.Game.FC;
using H1_ThirdPartyWalletAPI.Service.Game.Gemini;
using H1_ThirdPartyWalletAPI.Service.Game.GR;
using H1_ThirdPartyWalletAPI.Service.Game.IDN;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.API;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Interface;
using H1_ThirdPartyWalletAPI.Service.Game.JILI;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;
using H1_ThirdPartyWalletAPI.Service.Game.KS;
using H1_ThirdPartyWalletAPI.Service.Game.META;
using H1_ThirdPartyWalletAPI.Service.Game.MG;
using H1_ThirdPartyWalletAPI.Service.Game.MP;
using H1_ThirdPartyWalletAPI.Service.Game.MT;
using H1_ThirdPartyWalletAPI.Service.Game.NEXTSPIN;
using H1_ThirdPartyWalletAPI.Service.Game.OB;
using H1_ThirdPartyWalletAPI.Service.Game.PG.Service;
using H1_ThirdPartyWalletAPI.Service.Game.PME;
using H1_ThirdPartyWalletAPI.Service.Game.PP;
using H1_ThirdPartyWalletAPI.Service.Game.RCG2;
using H1_ThirdPartyWalletAPI.Service.Game.RGRICH;
using H1_ThirdPartyWalletAPI.Service.Game.RLG;
using H1_ThirdPartyWalletAPI.Service.Game.RSG;
using H1_ThirdPartyWalletAPI.Service.Game.RTG;
using H1_ThirdPartyWalletAPI.Service.Game.SEXY;
using H1_ThirdPartyWalletAPI.Service.Game.TP;
using H1_ThirdPartyWalletAPI.Service.Game.WM;
using H1_ThirdPartyWalletAPI.Service.Game.WS168;
using H1_ThirdPartyWalletAPI.Service.Game.XG;
using H1_ThirdPartyWalletAPI.Service.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using H1_ThirdPartyWalletAPI.Worker;
using H1_ThirdPartyWalletAPI.Worker.Game.BTI;
using H1_ThirdPartyWalletAPI.Worker.Game.CMD;
using H1_ThirdPartyWalletAPI.Worker.Game.CMD368;
using H1_ThirdPartyWalletAPI.Worker.Game.CR;
using H1_ThirdPartyWalletAPI.Worker.Game.EGSlot;
using H1_ThirdPartyWalletAPI.Worker.Game.Gemini;
using H1_ThirdPartyWalletAPI.Worker.Game.KS;
using H1_ThirdPartyWalletAPI.Worker.Game.MP;
using H1_ThirdPartyWalletAPI.Worker.Game.MT;
using H1_ThirdPartyWalletAPI.Worker.Game.NEXTSPIN;
using H1_ThirdPartyWalletAPI.Worker.Game.PME;
using H1_ThirdPartyWalletAPI.Worker.Game.PP;
using H1_ThirdPartyWalletAPI.Worker.Game.RCG2;
using H1_ThirdPartyWalletAPI.Worker.Game.RGRICH;
using H1_ThirdPartyWalletAPI.Worker.Game.RLG;
using H1_ThirdPartyWalletAPI.Worker.Game.RSG;
using H1_ThirdPartyWalletAPI.Worker.Game.IDN;
using H1_ThirdPartyWalletAPI.Worker.Schedule;
using Lab.GracefulShutdown.Net6;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.GameAPI.Service.Game.CR;
using ThirdPartyWallet.GameAPI.Service.Game.EGSlot;
using ThirdPartyWallet.GameAPI.Service.Game.Gemini;
using ThirdPartyWallet.GameAPI.Service.Game.IDN;
using ThirdPartyWallet.GameAPI.Service.Game.RGRICH;
using ThirdPartyWallet.GameAPI.Service.Game.RCG3;
using H1_ThirdPartyWalletAPI.Service.Game.RCG3;
using H1_ThirdPartyWalletAPI.Worker.Game.RCG3;
using H1_ThirdPartyWalletAPI.Service.Game.WE;
using ThirdPartyWallet.GameAPI.Service.Game.WE;
using H1_ThirdPartyWalletAPI.Worker.Game.WE;
using H1_ThirdPartyWalletAPI.Worker.Game.JILI;
using H1_ThirdPartyWalletAPI.Worker.Game.MG;
using ThirdPartyWallet.GameAPI.Service.Game.PS;
using H1_ThirdPartyWalletAPI.Service.Game.PS;
using H1_ThirdPartyWalletAPI.Worker.Game.PS;
using H1_ThirdPartyWalletAPI.Service.Common.DB.ClickHouse;
using ThirdPartyWallet.GameAPI.Service.Game.VA;
using H1_ThirdPartyWalletAPI.Service.Game.VA;
using H1_ThirdPartyWalletAPI.Worker.Game.VA;
using H1_ThirdPartyWalletAPI.Worker.Game.WM;
using H1_ThirdPartyWalletAPI.Worker.Game.SABA2;
using ThirdPartyWallet.GameAPI.Service.Game.SPLUS;
using H1_ThirdPartyWalletAPI.Worker.Game.SPLUS;
using H1_ThirdPartyWalletAPI.Service.Game.SPLUS;
using H1_ThirdPartyWalletAPI.Worker.Game.FC;
using H1_ThirdPartyWalletAPI.Worker.Game.WS168;

namespace H1_ThirdPartyWalletAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Config.Environment = env.EnvironmentName;
            Config.IsDevelopment = env.IsDevelopment();
            Config.JWT = configuration.GetSection("JWT").Get<JWT>();
            Config.GameAPI = configuration.GetSection("GameAPI").Get<GameAPI>();
            Config.OneWalletAPI = configuration.GetSection("OneWallet-API").Get<OneWalletAPI>();
            Config.Redis = configuration.GetSection("Redis").Get<Redis>();
            Config.CompanyToken = configuration.GetSection("CompanyToken").Get<CompanyToken>();
            Config.W1ScheduleConfig = configuration.GetSection("W1ScheduleConfig").Get<W1ScheduleConfig>();
            Config.RsgJackpotHistoryConfig = configuration.GetSection("RsgJackpotHistoryConfig").Get<RsgJackpotHistoryConfig>();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

            if (!Config.OneWalletAPI.Prefix_Key.Contains("prd"))
            {
                services.AddSwaggerDocument(settings =>
                {
                    settings.PostProcess = document =>
                    {
                        document.Info.Version = "Ver 1.0.1.3";
                        document.Info.Title = "W1 API";
                        document.Info.Description = "W1 API";
                    };

                    settings.AddSecurity("Bearer", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme()
                    {
                        Description = "JWT Authentication : Bearer {token}",
                        Name = "Authorization",
                        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                        Type = NSwag.OpenApiSecuritySchemeType.ApiKey
                    });
                });
            }
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddJwtBearer(options =>
              {
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuer = false,
                      ValidIssuer = Configuration["Jwt:Issuer"],
                      ValidateAudience = false,
                      ValidAudience = Configuration["Jwt:Audience"],
                      ValidateLifetime = true,
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:KEY"]))
                  };
              });

            #region Configure

            services.Configure<DBConnection>(Configuration.GetSection("OneWallet-API").GetSection("DBConnection"));

            #endregion Configure

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(Config.Redis.Configuration));
            services.AddSingleton<IDistributedLockFactory, RedLockFactory>(sp =>
            {
                var redis = sp.GetService<IConnectionMultiplexer>();
                var multiplexers = new List<RedLockMultiplexer>
                {
                    (ConnectionMultiplexer)redis.GetDatabase().Multiplexer
                };
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return RedLockFactory.Create(multiplexers, loggerFactory);
            });
            //Graceful shutdown
            services.AddSingleton<RequestProcessingFlag>();
            services.AddHostedService<GracefulShutdownService>();
            // health check
            services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis").AddCheck<DbHealthCheck>("db").AddCheck<CoravelHealthCheck>("coravel");
            services.AddScoped<ScopedLogTemp>();

            #region Common Service

            services.AddSingleton<ICacheDataService, CacheDataService>();
            services.AddMemoryCache();
            services.AddTransient<ApiHealthHandler>();
            services.AddScoped<HttpLogHandler>();
            services.AddSingleton(typeof(LogHelper<>));
            services
                .AddHttpClient("log")
                .AddHttpMessageHandler<HttpLogHandler>();
            //.AddHttpMessageHandler<ApiHealthHandler>();

            services.AddSingleton<ITransferWalletService, TransferWalletService>();
            services.AddSingleton<ISingleWalletService, SingleWalletService>();
            services.AddSingleton<IHttpService, HttpService>();
            services.AddSingleton<IDBService, DBService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<JWTService>();
            services.AddSingleton<IGamePlatformUserService, GamePlatformUserService>();
            services.AddSingleton<ICommonService, CommonService>();
            services.AddScoped<IApiHealthCheckService, ApiHealthCheckService>();

            #region IWalletDbConnectionStringManager

            services.AddSingleton<IWalletDbConnectionStringManager>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<WalletDbConnectionStringManager>>();
                var walletConnectionString = Config.OneWalletAPI.DBConnection.Wallet;
                return new WalletDbConnectionStringManager(logger, walletConnectionString.Master,
                    new[] { walletConnectionString.Read });
            });

            #endregion IWalletDbConnectionStringManager

            #region IBetLogsDbConnectionStringManager

            services.AddSingleton<IBetLogsDbConnectionStringManager>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<BetLogsDbConnectionStringManager>>();
                var betLogsConnectionString = sp.GetRequiredService<IOptions<DBConnection>>().Value.BetLog;
                return new BetLogsDbConnectionStringManager(logger, betLogsConnectionString.Master,
                    new[] { betLogsConnectionString.Read });
            });

            #endregion IBetLogsDbConnectionStringManager


            //ClickHouseDB
            services.AddSingleton<IBetSummaryReportDBService, BetSummaryReportDBService>();
            services.AddClickHouseDBServiceExtensions(Configuration);

            #endregion Common Service

            #region W1API Service

            services.AddSingleton<ITransferService, TransferService>();
            services.AddSingleton<IForwardGameService, ForwardGameService>();
            services.AddScoped<IRepairBetRecordService, RepairBetRecordService>();
            services.AddScoped<IBetRecordService, BetRecordService>();
            services.AddScoped<IWalletSessionService, WalletSessionService>();
            services.AddScoped<IOnlineUserService, OnlineUserService>();
            services.AddSingleton<IJackpotHistoryService, JackpotHistoryService>();
            services.AddSingleton<IKickAllUserService, KickAllUserService>();

            #endregion W1API Service

            #region Game API Service

            services.AddSingleton<IGameApiService, GameApiService>();
            services.AddScoped<IRcgStealthilyApiService, RCGApiService>();
            services.AddScoped<IMGApiService, MGApiService>();
            services.AddScoped<IDSApiService, DSApiService>();
            services.AddScoped<IPGApiService, PGApiService>();
            services.AddScoped<IAEApiService, AEApiService>();
            services.AddScoped<IRSGApiService, RSGApiService>();
            services.AddScoped<IRLGApiService, RLGApiService>();
            services.AddScoped<IJDBApiService, JDBApiService>();
            services.AddScoped<IH1ApiService, H1ApiService>();
            services.AddScoped<IStreamerApiService, STREAMERApiService>();
            services.AddScoped<ITPApiService, TPApiService>();
            services.AddScoped<IRTGApiService, RTGApiService>();
            services.AddScoped<IJILIApiService, JILIApiService>();
            services.AddScoped<IJokerApiService, JokerApiService>();
            services.AddScoped<IGRApiService, GRApiService>();
            services.AddScoped<IMETAApiService, METAApiService>();
            services.AddScoped<IOBApiService, OBApiService>();
            services.AddScoped<IWMApiService, WMApiService>();
            services.AddScoped<ISEXYApiService, SEXYApiService>();
            services.AddScoped<IXGApiService, XGApiService>();
            services.AddScoped<INEXTSPINApiService, NEXTSPINApiService>();
            services.AddScoped<IPPApiService, PPApiService>();
            services.AddScoped<IFCApiService, FCApiService>();
            services.AddScoped<IWS168ApiService, WS168ApiService>();
            services.AddScoped<ISaba2ApiService, SABA2ApiService>();
            services.AddScoped<IMTApiService, MTApiService>();
            services.AddSingleton<IPMEApiService, PMEApiService>();
            services.AddScoped<IMPApiService, MPApiService>();
            services.AddSingleton<ICMDApiService, CMDApiService>();
            services.AddScoped<IKSApiService, KSApiService>();
            services.AddSingleton<IBTIApiService, BTIApiService>();
            services.AddSingleton<IRCG2ApiService, RCG2ApiService>();
            services.AddGeminiApiService(Configuration);
            services.AddRGRICHApiService(Configuration);
            services.AddRCG3ApiService(Configuration);
            services.AddEGSlotApiService(Configuration);
            services.AddWEApiService(Configuration);
            services.AddCRApiService(Configuration);
            services.AddIDNApiService(Configuration);
            services.AddPsApiService(Configuration);
            services.AddVAApiService(Configuration);
            services.AddSPLUSApiService(Configuration);
            #endregion Game API Service

            #region BetlogsDBService
            services.AddSingleton<INextSpinDBService, NextSpinDBService>();
            services.AddSingleton<IBTIDBService, BTIDBService>();
            services.AddSingleton<IDsDBService, DsDBService>();
            services.AddSingleton<IGrDBService, GrDBService>();
            services.AddSingleton<IMPDBService, MPDBService>();
            services.AddSingleton<IMTDBService, MTDBService>();
            services.AddSingleton<IRcgDBService, RcgDBService>();
            services.AddSingleton<IRcg2DBService, Rcg2DBService>();
            services.AddSingleton<IRtgDBService, RtgDBService>();
            services.AddSingleton<ISystemParameterDbService, SystemParameterDBService>();
            services.AddSingleton<IJdbDBService, JdbDBService>();
            services.AddSingleton<IJILIDBService, JILIDBService>();
            services.AddSingleton<IMETADBService, METADBService>();
            services.AddSingleton<IPMEDBService, PMEDBService>();
            services.AddSingleton<ITpDBService, TpDBService>();
            services.AddSingleton<ISummaryDBService, SummaryDBService>();
            services.AddSingleton<IAeDBService, AeDBService>();
            services.AddSingleton<ICMDDBService, CMDDBService>();
            services.AddSingleton<IFCDBService, FCDBService>();
            services.AddSingleton<IKSDBService, KSDBService>();
            services.AddSingleton<IOBDBService, OBDBService>();
            services.AddSingleton<IPPDBService, PPDBService>();
            services.AddSingleton<IWMDBService, WMDBService>();
            services.AddSingleton<IMgDbService, MgDbService>();
            services.AddSingleton<IRlgDbService, RlgDbService>();
            services.AddSingleton<ISabaDbService, SabaDbService>();
            services.AddScoped<IXgDBService, XgDBService>();
            services.AddScoped<ISEXYDBService, SEXYDBService>();
            services.AddScoped<IWS168DBService, WS168DBService>();
            services.AddScoped<IRsgDBService, RsgDBService>();
            services.AddScoped<IPgDBService, PgDBService>();
            services.AddScoped<IJokerDBService, JokerDBService>();
            services.AddSingleton<IGameReportDBService, GameReportDBService>();
            services.AddSingleton<IGeminiDBService, GeminiDBService>();
            services.AddSingleton<IRGRICHDBService, RGRICHDBService>();
            services.AddSingleton<IRCG3DBService, RCG3DBService>();
            services.AddSingleton<IEGSlotDBService, EGSlotDBService>();
            services.AddSingleton<ICRDBService, CRDBService>();
            services.AddSingleton<IWEDBService, WEDBService>();
            services.AddSingleton<IGameTypeMappingDBService, GameTypeMappingDBService>();
            services.AddSingleton<IIDNDBService, IDNDBService>();
            services.AddSingleton<IPSDBService, PSDBService>();
            services.AddSingleton<IVADBService, VADBService>();
            services.AddSingleton<ISPLUSDBService, SPLUSDBService>();
            #endregion BetlogsDBService

            #region Schedule
            services.AddScheduler();
            services.AddSingleton<TestSchedule>();
            services.AddSingleton<RsgJackpotHistorySchedule>();
            services.AddScoped<RsgSlotRecordSchedule>();
            services.AddScoped<RsgFishRecordSchedule>();
            services.AddScoped<RsgRecordSummarySchedule>();
            services.AddSingleton<TransferRecordSchedule>();
            services.AddSingleton<RcgTransactionSchedule>();
            services.AddSingleton<RcgRepairSchedule>();
            services.AddSingleton<RcgTransactionSchedule>();
            services.AddSingleton<RcgRecordSchedule>();
            services.AddScoped<RcgReportSchedule>();
            services.AddSingleton<JdbRecordSchedule>();
            services.AddScoped<JdbReportSchedule>();
            services.AddScoped<JdbAuditSchedule>();
            services.AddScoped<JdbRecordSummarySchedule>();
            services.AddSingleton<DsRecordSchedule>();
            services.AddScoped<DsReportSchedule>();
            services.AddScoped<DsAuditSchedule>();
            services.AddScoped<DsRecordSummarySchedule>();
            services.AddSingleton<MgRecordSchedule>();
            services.AddScoped<MgReportSchedule>();
            services.AddSingleton<PgRecordSchedule>();
            services.AddScoped<PgReportSchedule>();
            services.AddScoped<PgAuditSchedule>();
            services.AddSingleton<RsgReportSchedule>();
            services.AddSingleton<RlgRecordSchedule>();
            services.AddSingleton<RlgRecordFailoverSchedule>();
            services.AddSingleton<RlgReportSchedule>();
            services.AddSingleton<RlgAuditSchedule>();
            services.AddSingleton<RsgAuditSchedule>();
            services.AddSingleton<H1RsgRecordSchedule>();
            services.AddSingleton<AeRecordSchedule>();
            services.AddSingleton<AeReportSchedule>();
            services.AddSingleton<AeRecordSummarySchedule>();
            services.AddSingleton<WalletSchedule>();
            services.AddSingleton<SessionWithdrawSchedule>();
            services.AddSingleton<SessionWithdrawQueueSchedule>();
            services.AddSingleton<SessionRefundQueueSchedule>();
            services.AddSingleton<SessionMoveSchedule>();
            services.AddSingleton<SetOnlineUserSchedule>();
            services.AddSingleton<SessionRecordSchedule>();
            services.AddSingleton<ApiHealthCheckSchedule>();
            services.AddSingleton<KickIdleUserSchedule>();
            services.AddSingleton<TpRecordSchedule>();
            services.AddSingleton<TpReportSchedule>();
            services.AddSingleton<TpRecordSummarySchedule>();
            services.AddSingleton<TpAuditSchedule>();
            services.AddSingleton<RtgRecordSchedule>();
            services.AddSingleton<RtgReportSchedule>();
            services.AddSingleton<RtgAuditSchedule>();
            services.AddSingleton<JiliRecordSchedule>();
            services.AddSingleton<JiliReportSchedule>();
            services.AddSingleton<JiliAuditSchedule>();
            services.AddSingleton<JokerRecordSchedule>();
            services.AddSingleton<JokerReportSchedule>();
            services.AddSingleton<JokerAuditSchedule>();
            services.AddSingleton<JokerRecordSummarySchedule>();
            services.AddSingleton<MetaRecordSchedule>();
            services.AddSingleton<MetaReportSchedule>();
            services.AddSingleton<GrRecordSchedule>();
            services.AddSingleton<GrReportSchedule>();
            services.AddSingleton<GrRecordSummarySchedule>();
            services.AddSingleton<GrAuditSchedule>();
            services.AddSingleton<OBRecordSchedule>();
            services.AddSingleton<OBReportSchedule>();
            services.AddSingleton<OBAuditSchedule>();
            services.AddSingleton<WMRecordSchedule>();
            services.AddSingleton<WMReportSchedule>();
            services.AddSingleton<WMAuditSchedule>();
            services.AddSingleton<SexyRecordSchedule>();
            services.AddSingleton<SexyReportSchedule>();
            services.AddSingleton<SexyAuditSchedule>();
            services.AddSingleton<XgRecordSchedule>();
            services.AddSingleton<XgReportSchedule>();
            services.AddSingleton<XgAuditSchedule>();
            services.AddSingleton<NextSpinRecordSchedule>();
            services.AddSingleton<NextSpinReportSchedule>();
            services.AddSingleton<NextSpinRecordSummarySchedule>();
            services.AddSingleton<PPRecordSchedule>();
            services.AddSingleton<PPReportSchedulecs>();
            services.AddSingleton<FcRecordSchedule>();
            services.AddSingleton<FcReportSchedule>();
            services.AddSingleton<FcAuditSchedule>();
            services.AddSingleton<FcRecordSummarySchedule>();
            services.AddSingleton<FcRecordFailoverSchedule>();
            services.AddSingleton<WS168RecordSchedule>();
            services.AddSingleton<WS168ReportSchedule>();
            services.AddSingleton<MgAuditSchedule>();
            services.AddSingleton<CoravelHealthCheckSchedule>();
            services.AddSingleton<Saba2RecordSchedule>();
            services.AddSingleton<Saba2ReportSchedule>();
            services.AddSingleton<Saba2AuditSchedule>();
            services.AddSingleton<PPAuditSchedule>();
            services.AddSingleton<MTRecordSchedule>();
            services.AddSingleton<MTReportSchedule>();
            services.AddSingleton<MTAuditSchedule>();
            services.AddSingleton<PMERecordSchedule>();
            services.AddSingleton<PMERecordFailoverSchedule>();
            services.AddSingleton<PMEReportSchedule>();
            services.AddSingleton<PMEAuditSchedule>();
            services.AddSingleton<MPRecordSchedule>();
            services.AddSingleton<MPReportSchedule>();
            services.AddSingleton<MPAuditSchedule>();
            services.AddSingleton<KSRecordSchedule>();
            services.AddSingleton<KSReportSchedule>();
            services.AddSingleton<KSAuditSchedule>();
            services.AddSingleton<KSRecordFailoverSchedule>();
            services.AddSingleton<BTIRecordSchedule>();
            services.AddSingleton<BTIReportSchedule>();
            services.AddSingleton<BTIAuditSchedule>();
            services.AddSingleton<CMDReportSchedule>();
            services.AddSingleton<CMDRecordSchedule>();
            services.AddSingleton<CMDRecordFailoverSchedule>();
            services.AddSingleton<CMDRecordSummarySchedule>();
            services.AddSingleton<Rcg2RecordSchedule>();
            services.AddSingleton<Rcg2ReportSchedule>();
            services.AddSingleton<Rcg2AuditSchedule>();
            services.AddSingleton<PlatformHealthCheckSchedule>();
            services.AddSingleton<GeminiRecordSchedule>();
            services.AddSingleton<GeminiRecordSummarySchedule>();
            services.AddSingleton<GEMINIReportSchedule>();
            services.AddSingleton<GEMINIAuditSchedule>();
            services.AddSingleton<RGRICHRecordSchedule>();
            services.AddSingleton<RGRICHRecordSummarySchedule>();
            services.AddSingleton<RGRICHReportSchedule>();
            services.AddSingleton<RGRICHAuditSchedule>();
            services.AddSingleton<RGRICHRecordFailoverSchedule>();
            services.AddSingleton<Rcg3RecordSchedule>();
            services.AddSingleton<Rcg3ReportSchedule>();
            services.AddSingleton<Rcg3AuditSchedule>();
            services.AddSingleton<EGSlotRecordSchedule>();
            services.AddSingleton<EGSlotRecordSummarySchedule>();
            services.AddSingleton<EGSlotReportSchedule>();
            services.AddSingleton<EGSlotAuditSchedule>();
            services.AddSingleton<CRRecordSchedule>();
            services.AddSingleton<CRRecordSummarySchedule>();
            services.AddSingleton<CRReportSchedule>();
            services.AddSingleton<WERecordSchedule>();
            services.AddSingleton<WERecordSummarySchedule>();
            services.AddSingleton<WEReportSchedule>();
            services.AddSingleton<WEAuditSchedule>();
            //services.AddSingleton<CRAuditSchedule>();
            services.AddSingleton<CRRecordFailoverSchedule>();
            services.AddSingleton<NextSpinRecordFailoverSchedule>();
            services.AddSingleton<JiliRecordSummarySchedule>();
            services.AddSingleton<PPRecordSummarySchedule>();
            services.AddSingleton<MgRecordSummarySchedule>();
            services.AddSingleton<MgRecordFailoverSchedule>();
            services.AddSingleton<IDNRecordSchedule>();
            services.AddSingleton<IDNRecordSummarySchedule>();
            services.AddSingleton<IDNReportSchedule>();
            services.AddSingleton<IDNAuditSchedule>();
            services.AddSingleton<IDNRecordFailoverSchedule>();
            services.AddSingleton<PSRecordSchedule>();
            services.AddSingleton<PSFishRecordSchedule>();
            services.AddSingleton<PSAuditSchedule>();
            services.AddSingleton<PSRecordSummarySchedule>();
            services.AddSingleton<PSReportSchedule>();
            services.AddSingleton<PMERecordSummarySchedule>();
            services.AddSingleton<VARecordSchedule>();
            services.AddSingleton<VARecordSummarySchedule>();
            services.AddSingleton<VARecordFailoverSchedule>();
            services.AddSingleton<VAReportSchedule>();
            services.AddSingleton<VAAuditSchedule>();
            services.AddSingleton<Rcg2RecordSummarySchedule>();
            services.AddSingleton<Rcg3RecordSummarySchedule>();
            services.AddSingleton<WMRecordSummarySchedule>();
            services.AddSingleton<RlgRecordSummarySchedule>();
            services.AddSingleton<Saba2RecordSummarySchedule>();
            services.AddSingleton<SPLUSAuditSchedule>();
            services.AddSingleton<SPLUSRecordFailoverSchedule>();
            services.AddSingleton<SPLUSRecordSchedule>();
            services.AddSingleton<SPLUSRecordSummarySchedule>();
            services.AddSingleton<SPLUSReportSchedule>();
            services.AddSingleton<MTRecordSummarySchedule>();
            services.AddSingleton<WS168RecordSummarySchedule>();
            services.AddSingleton<WS168RecordFailoverSchedule>();
            services.AddSingleton<NextSpinAuditSchedule>();
            #endregion Schedule

            #region Game Interface Service
            services.AddSingleton<IGameInterfaceService, GameInterfaceService>();
            services.AddSingleton<GameRecordService>();
            services.AddScoped<IJdbInterfaceService, JDB_Service>();
            services.AddScoped<IRsgH1InterfaceService, RSG_H1RecordService>();
            services.AddScoped<IRcgInterfaceService, RCG_RecordService>();
            //services.AddScoped<IRcgSessionRecordService, RcgSessionRecordService>();
            services.AddScoped<IMgInterfaceService, MG_RecordService>();
            services.AddScoped<IDsInterfaceService, DS_RecordService>();
            services.AddScoped<IPGInterfaceService, PG_RecordService>();
            services.AddScoped<IAeInterfaceService, AE_RecordService>();
            services.AddScoped<IRlgInterfaceService, RLG_RecordService>();
            services.AddScoped<IStreamerInterfaceService, STREAMER_InterfaceService>();
            services.AddScoped<ITPInterfaceService, TP_InterfaceService>();
            services.AddScoped<IRtgInterfaceService, RTG_RecordService>();
            services.AddScoped<IJILIInterfaceService, JILI_RecordService>();
            services.AddScoped<IJOKER_InterfaceService, JOKER_InterfaceService>();
            services.AddScoped<IMETAInterfaceService, META_RecordService>();
            services.AddScoped<IGRInterfaceService, GR_RecordService>();
            services.AddScoped<IOBInterfaceService, OB_RecordService>();
            services.AddScoped<IWMInterfaceService, WM_RecordService>();
            services.AddScoped<ISEXYInterfaceService, SEXY_RecordService>();
            services.AddScoped<IXGInterfaceService, XG_RecordService>();
            services.AddScoped<INEXTSPIN_InterfaceService, NEXTSPIN_InterfaceService>();
            services.AddScoped<IPPInterfaceService, PP_InterfaceService>();
            services.AddScoped<IFCInterfaceService, FC_RecordService>();
            services.AddScoped<IWS168InterfaceService, WS168_RecordService>();
            services.AddScoped<ISaba2InterfaceService, SABA2_InterfaceService>();
            services.AddScoped<IMTInterfaceService, MT_InterfaceService>();
            services.AddSingleton<IPMEInterfaceService, PME_InterfaceService>();
            services.AddScoped<IMPInterfaceService, MP_InterfaceService>();
            services.AddSingleton<ICMDInterfaceService, CMD_InterfaceService>();
            services.AddScoped<IKSInterfaceService, KS_RecordService>();
            services.AddSingleton<IBTIInterfaceService, BTI_InterfaceService>();
            services.AddSingleton<IRCG2InterfaceService, RCG2_InterfaceService>();
            services.AddSingleton<IGeminiInterfaceService, Gemini_InterfaceService>();
            services.AddSingleton<IRGRICHInterfaceService, RGRICH_InterfaceService>();
            services.AddSingleton<IRCG3InterfaceService, RCG3_InterfaceService>();
            services.AddSingleton<IEGSlotInterfaceService, EGSlot_InterfaceService>();
            services.AddSingleton<ICRInterfaceService, CR_InterfaceService>();
            services.AddSingleton<IWEInterfaceService, WE_InterfaceService>();
            services.AddSingleton<IPsInterfaceService, Ps_InterfaceService>();
            services.AddSingleton<IIDNInterfaceService, IDN_InterfaceService>();
            services.AddSingleton<IVAInterfaceService, VA_InterfaceService>();
            services.AddSingleton<ISPLUS_InterfaceService, SPLUS_InterfaceService>();
            #endregion Game Interface Service
             
            #region JDB Service

            //services.AddScoped<ApiActionFactory>();
            //services.AddScoped<IActionService, BetService>();
            //services.AddScoped<IActionService, CancelService>();
            //services.AddScoped<IActionService, BalanceService>();
            //services.AddScoped<IGameBetService, SlotGameBetService>();
            //services.AddScoped<IGameBetService, ArcadeGameBetService>();
            //services.AddScoped<IGameBetService, LotteryGameBetService>();
            //services.AddScoped<IGameBetService, FishGameBetService>();

            #endregion JDB Service
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            var Identity = Environment.GetEnvironmentVariable("AP_IDENTITY");
            //app.UseMiddleware<DecompressionMiddleware>();
            app.UseDeveloperExceptionPage();
            // app.UseMiddleware<LogMiddleware>();
            app.UseMiddleware<LogMiddleware>();
            app.UseMiddleware<RequestMonitorMiddleware>();
            #region for jdb Setting
            app.UseWhen(x => x.Request.Path.StartsWithSegments("/jdb/api"), app =>
            {
                //    //if (env.EnvironmentName == "Local")
                //    //{
                //    //    app.UseMiddleware<URLRewriteMiddleware>();
                //    //}
                //    //else
                //    //{
                //    //    app.UseCryptMiddleware();
                //    //}
                app.UseMiddleware<ErrorHandlingMiddleware>();
            });

            #endregion
            if (!Config.OneWalletAPI.Prefix_Key.Contains("prd"))
            {
                app.UseOpenApi();    // strat OpenAPI document
                app.UseSwaggerUi3(); // strat Swagger UI
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            var provider = app.ApplicationServices;
            provider.UseScheduler(scheduler =>
            {
                //=========todo test area==================
                //=========test area End===================
                if (Config.OneWalletAPI.Redis_PreKey != "Local")
                {
                    scheduler.Schedule<CoravelHealthCheckSchedule>().EverySecond().PreventOverlapping("CoravelHealthCheckSchedule").RunOnceAtStart();

                    List<string> openGame = new List<string>(Config.OneWalletAPI.OpenGame.Split(','));
                    switch (Identity)
                    {
                        case "WORKER":
                            if (Config.OneWalletAPI.WalletMode == "SingleWallet")
                            {
                                //單一錢包檢查RCG交易排程
                                scheduler.Schedule<RcgRepairSchedule>().Cron("*/5 * * * *").PreventOverlapping("RcgRepairScheduleLock");
                                //單一錢包RCG交易寫入排程 REDIS -> DB
                                scheduler.Schedule<RcgTransactionSchedule>().EverySeconds(1).PreventOverlapping("RcgTransactionScheduleLock").RunOnceAtStart();
                                //單一錢包錢包餘額寫入排程 REDIS -> DB
                                scheduler.Schedule<WalletSchedule>().EverySeconds(1).PreventOverlapping("WalletScheduleLock").RunOnceAtStart();
                            }
                            else
                            {
                                //檢查轉帳記錄排程-處理狀態未完成記錄
                                scheduler.Schedule<TransferRecordSchedule>().EveryMinute().PreventOverlapping("TransferRecordScheduleLock").RunOnceAtStart();
                                //檢查遊戲館API健康資訊排程
                                scheduler.Schedule<ApiHealthCheckSchedule>().EverySeconds(10);
                                //處理Redis沒有的洗分與退款
                                scheduler.Schedule<SessionWithdrawSchedule>().EverySeconds(5).PreventOverlapping("SessionScheduleLock");
                                //將已經退款完成的SESSION移到歷史區
                                scheduler.Schedule<SessionMoveSchedule>().EverySeconds(30).PreventOverlapping("SessionMoveScheduleLock").RunOnceAtStart();
                                //踢掉閒置使用者
                                scheduler.Schedule<KickIdleUserSchedule>().EverySecond().PreventOverlapping("KickIdleUserScheduleLock").RunOnceAtStart();
                                //遊戲商檢康度檢查排程
                                scheduler.Schedule<PlatformHealthCheckSchedule>().EveryTenSeconds().RunOnceAtStart();
                            }
                            break;

                        case "OPERATOR":
                            //處理洗分清單
                            scheduler.Schedule<SessionWithdrawQueueSchedule>().EverySeconds(30).PreventOverlapping("SessionWithdrawQueueScheduleLock");
                            //推送有更新的SESSION到平台方
                            scheduler.Schedule<SessionRefundQueueSchedule>().EverySeconds(30).PreventOverlapping("SessionRefundQueueScheduleLock");
                            //todo 平台未實作先不執行排程
                            //scheduler.Schedule<SessionRecordSchedule>().EveryMinute().PreventOverlapping("SessionRecordScheduleLock").RunOnceAtStart();
                            break;

                        case "ONLINEUSERLIST":
                            //更新遊戲線上使用者清單排程
                            scheduler.Schedule<SetOnlineUserSchedule>().Cron("*/2 * * * *").PreventOverlapping("SetOnlineUserScheduleLock").RunOnceAtStart();
                            break;

                        case "RCG":
                            //處理RCG記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<RcgRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("RcgRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<RcgReportSchedule>().Cron("0,20,40 * * * *").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case nameof(Platform.RCG2):
                            //處理RCG2記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<Rcg2RecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("Rcg2RecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<Rcg2RecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("Rcg2RecordSummaryScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<Rcg2ReportSchedule>().Cron("0,20,40 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("Rcg2ReportScheduleLock");
                                scheduler.Schedule<Rcg2AuditSchedule>().EveryFifteenMinutes().PreventOverlapping("Rcg2AuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case "JDB":
                            //處理JDB記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<JdbRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("JdbRecordScheduleLock").RunOnceAtStart();
                                // 五分鐘匯總
                                scheduler.Schedule<JdbRecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("JdbRecordSummaryScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<JdbReportSchedule>().Cron("10 14 * * *").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<JdbAuditSchedule>().Cron("20 14 * * *").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case "PG":
                            //處理PG記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<PgRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("PgRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<PgReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("PgReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<PgAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("PgAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case "DS":
                            //處理DS記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<DsRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("DsRecordScheduleLock").RunOnceAtStart();
                                // 五分鐘匯總
                                scheduler.Schedule<DsRecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("DsRecordSummaryScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<DsReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("DsReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<DsAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("DsAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case "RSG":
                            //處理RSG記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                // 老虎機
                                scheduler.Schedule<RsgSlotRecordSchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("RsgSlotRecordScheduleLock").RunOnceAtStart();
                                // 魚機
                                scheduler.Schedule<RsgFishRecordSchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("RsgFishRecordScheduleLock").RunOnceAtStart();
                                // 五分鐘匯總
                                scheduler.Schedule<RsgRecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("RsgRecordSummaryScheduleLock").RunOnceAtStart();
                                // 5分鐘匯總報表
                                scheduler.Schedule<RsgReportSchedule>().EverySeconds(10).PreventOverlapping("RsgReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                // 自動補單
                                scheduler.Schedule<RsgAuditSchedule>().EverySeconds(10).PreventOverlapping("RsgAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                                // 定期取得皇電中獎清單
                                scheduler.Schedule<RsgJackpotHistorySchedule>().EverySeconds(30).PreventOverlapping("RsgJackpotHistoryScheduleLock").Zoned(TimeZoneInfo.Local).RunOnceAtStart();
                            }
                            break;

                        case "PP":
                            //處理PP記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<PPRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("PPRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<PPReportSchedulecs>().Cron("0,20,40 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("PPReportSchedulecsLock");
                                scheduler.Schedule<PPAuditSchedule>().EveryFifteenMinutes().PreventOverlapping("PPAuditScheduleLock").Zoned(TimeZoneInfo.Local);

                                scheduler.Schedule<PPRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("PPRecordSummarySchedule").RunOnceAtStart();

                            }
                            break;

                        case "MG":
                            //處理MG記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<MgRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("MgRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<MgRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("MgRecordSummaryScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<MgReportSchedule>().Cron("0,20,40 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("MgReportScheduleLock");
                                scheduler.Schedule<MgAuditSchedule>().Cron("10,30,50 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("MgAuditScheduleLock");
                                scheduler.Schedule<MgRecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("MgRecordFailoverScheduleLock");
                            }
                            break;

                        case "AE":
                            //處理AE記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<AeRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("AeRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<AeReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("AeReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                // 五分鐘匯總
                                scheduler.Schedule<AeRecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("AeRecordSummaryScheduleLock").RunOnceAtStart();
                            }
                            break;

                        case "RLG":
                            //處理RLG記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<RlgRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("RlgRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<RlgRecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("RlgRecordFailoverSchedule");
                                scheduler.Schedule<RlgReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("RlgReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<RlgAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("RlgAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                                // 五分鐘匯總
                                scheduler.Schedule<RlgRecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("RlgRecordSummaryScheduleLock").RunOnceAtStart();

                            }
                            break;

                        case "TP":
                            //處理TP記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<TpRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("TpRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<TpReportSchedule>().Cron("0,20,40 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("TpReportScheduleLock");
                                scheduler.Schedule<TpAuditSchedule>().Cron("10,30,50 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("TpAuditSchedule");
                                // 五分鐘匯總
                                scheduler.Schedule<TpRecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("TpRecordSummaryScheduleLock").RunOnceAtStart();
                            }
                            break;

                        case "RTG":
                            //處理RTG記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<RtgRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("RtgRecordScheduleLock");
                                scheduler.Schedule<RtgReportSchedule>().Cron("10 01 * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("RtgReportScheduleLock");
                                scheduler.Schedule<RtgAuditSchedule>().Cron("20 01 * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("RtgAuditSchedule");
                            }
                            break;

                        case "JILI":
                            //處理JILI記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<JiliRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("JiliRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<JiliReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("JiliReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<JiliAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("JiliAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<JiliRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("GeminiRecordSummaryScheduleLock").RunOnceAtStart();

                            }
                            break;

                        case "JOKER":
                            //處理JOKER記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<JokerRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("JokerRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<JokerReportSchedule>().Cron("0,20,40 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("JokerReportScheduleLock");
                                scheduler.Schedule<JokerAuditSchedule>().Cron("10,30,50 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("JokerAuditScheduleLock");
                                scheduler.Schedule<JokerRecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("JokerRecordSummaryScheduleLock").RunOnceAtStart();
                            }
                            break;

                        case "META":
                            //處理META記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<MetaRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("MetaRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<MetaReportSchedule>().Cron("0,20,40 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("MetaReportScheduleLock");
                                //scheduler.Schedule<MetaAuditSchedule>().Cron("20 01 * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("MetaAuditSchedule");
                            }
                            break;

                        case "GR":
                            //處理GR記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<GrRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("GrRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<GrReportSchedule>().Cron("10 02 * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("GrReportScheduleLock");
                                // 五分鐘匯總
                                scheduler.Schedule<GrRecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("GrRecordSummaryScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<GrAuditSchedule>().Cron("20 02 * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("GrAuditScheduleLock");
                            }
                            break;

                        case "OB":
                            //處理OB記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<OBRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("OBRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<OBReportSchedule>().Cron("10 01 * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("OBReportScheduleLock");
                                scheduler.Schedule<OBAuditSchedule>().Cron("20 01 * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("OBAuditScheduleLock");
                            }
                            break;

                        case "WM":
                            //處理WM記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<WMRecordSchedule>().EverySeconds(30).Zoned(TimeZoneInfo.Local).PreventOverlapping("WMRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<WMRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("WMRecordSummaryScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<WMReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("WMReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<WMAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("WMAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case "SEXY":
                            //處理SEXY記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<SexyRecordSchedule>().EverySeconds(30).Zoned(TimeZoneInfo.Local).PreventOverlapping("SexyRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<SexyReportSchedule>().Cron("20,40 * * * *").PreventOverlapping("SexyReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<SexyAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("SexyAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case "XG":
                            //處理XG記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<XgRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("XgRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<XgReportSchedule>().Cron("0,20,40 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("XgReportScheduleLock");
                                scheduler.Schedule<XgAuditSchedule>().Cron("10,30,50 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("XgAuditScheduleLock");
                            }
                            break;

                        case "NEXTSPIN":
                            //處理NEXTSPIN記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<NextSpinRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("NextSpinRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<NextSpinReportSchedule>().Cron("5,25,45 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("NextSpinReportScheduleLock");
                                scheduler.Schedule<NextSpinRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("NextSpinRecordSummarySchedule").RunOnceAtStart();
                                scheduler.Schedule<NextSpinRecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("NextSpinRecordFailoverSchedule");
                                //// 遊戲注單補單排程
                                scheduler.Schedule<NextSpinAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("NextSpinAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case "FC":
                            //處理FC記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<FcRecordSchedule>().EverySeconds(30).Zoned(TimeZoneInfo.Local).PreventOverlapping("FcRecordScheduleLock").RunOnceAtStart();
                                // 一般拉單例外處理排程
                                scheduler.Schedule<FcRecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("FcRecordFailoverScheduleLock");
                                scheduler.Schedule<FcReportSchedule>().Cron("10 13 * * *").PreventOverlapping("FcReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<FcAuditSchedule>().Cron("20 13 * * *").PreventOverlapping("FcAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                                // 五分鐘匯總
                                scheduler.Schedule<FcRecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("FcRecordSummaryScheduleLock").RunOnceAtStart();
                            }
                            break;

                        case "WS168":
                            //處理WS168記錄與報表
                            if (openGame.Contains(Identity))
                            {

                                scheduler.Schedule<WS168RecordSchedule>().EverySeconds(30).Zoned(TimeZoneInfo.Local).PreventOverlapping("WS168RecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<WS168RecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("WS168RecordFailoverScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<WS168RecordSummarySchedule>().EverySeconds(30).Zoned(TimeZoneInfo.Local).PreventOverlapping("WS168RecordSummaryScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<WS168ReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("WS168ReportScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;
                        case "SABA2":
                            //處理SABA2記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<Saba2RecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("Saba2RecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<Saba2ReportSchedule>().Cron("0 13 * * *").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<Saba2AuditSchedule>().Cron("10 13 * * *").Zoned(TimeZoneInfo.Local);
                                // 五分鐘匯總
                                scheduler.Schedule<Saba2RecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("Saba2RecordSummaryScheduleLock").RunOnceAtStart();

                            }
                            break;

                        case nameof(Platform.MT):
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<MTRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("MTRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<MTReportSchedule>().Cron("0 1 * * *").PreventOverlapping("MTReportSchedule").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<MTAuditSchedule>().Cron("20 1 * * *").PreventOverlapping("MTAuditSchedule").Zoned(TimeZoneInfo.Local);
                                // 五分鐘匯總
                                scheduler.Schedule<MTRecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("MTRecordSummaryScheduleLock").RunOnceAtStart();
                            }
                            break;

                        case "PME":
                            //處理PME記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<PMERecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("PMERecordSchedule").RunOnceAtStart();
                                scheduler.Schedule<PMERecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("PMERecordFailoverSchedule");
                                scheduler.Schedule<PMEReportSchedule>().Cron("0,20,40 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("PMEReportSchedule");
                                scheduler.Schedule<PMEAuditSchedule>().EveryFifteenMinutes().PreventOverlapping("PMEAuditSchedule").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<PMERecordSummarySchedule>().EverySeconds(10).Zoned(TimeZoneInfo.Local).PreventOverlapping("PMERecordSummaryScheduleLock").RunOnceAtStart();
                            }
                            break;

                        case nameof(Platform.MP):
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<MPRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("MPRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<MPReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("MPReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<MPAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("MPAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case nameof(Platform.KS):
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<KSRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("KSRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<KSReportSchedule>().Cron("0 1 * * *").PreventOverlapping("KSReportSchedule").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<KSAuditSchedule>().Cron("20 1 * * *").PreventOverlapping("KSAuditSchedule").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<KSRecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("KSRecordFailoverSchedule");
                            }
                            break;

                        case nameof(Platform.BTI):
                            //處理BTI記錄與報表
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<BTIRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("BTIRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<BTIReportSchedule>().Cron("0,20,40 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("BTIReportScheduleLock");
                                scheduler.Schedule<BTIAuditSchedule>().EveryFifteenMinutes().PreventOverlapping("BTIAuditSchedule").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case nameof(Platform.CMD368):
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<CMDRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("CMDRecordSchedule").RunOnceAtStart();
                                scheduler.Schedule<CMDReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("CMDReportSchedule").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<CMDRecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("CMDRecordFailoverSchedule");
                                scheduler.Schedule<CMDRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("CMDRecordSummaryScheduleLock").RunOnceAtStart();
                            }
                            break;

                        case nameof(Platform.GEMINI):
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<GeminiRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("GeminiRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<GeminiRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("GeminiRecordSummaryScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<GEMINIReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("GEMINIReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<GEMINIAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("GEMINIAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case nameof(Platform.RGRICH):
                            if (openGame.Contains(Identity))
                            {
                                // 一般拉單排程
                                scheduler.Schedule<RGRICHRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("RGRICHRecordScheduleLock").RunOnceAtStart();
                                // 一般拉單例外處理排程
                                scheduler.Schedule<RGRICHRecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("RGRICHRecordFailoverScheduleLock");
                                // 遊戲注單後匯總排程(5分鐘彙總給H1的)
                                scheduler.Schedule<RGRICHRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("RGRICHRecordSummaryScheduleLock").RunOnceAtStart();
                                // 遊戲注單報表排程(W1和遊戲商)
                                scheduler.Schedule<RGRICHReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("RGRICHReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                // 遊戲注單補單排程
                                scheduler.Schedule<RGRICHAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("RGRICHAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;
                        case nameof(Platform.RCG3):
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<Rcg3RecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("Rcg3RecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<Rcg3RecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("Rcg3RecordSummaryScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<Rcg3ReportSchedule>().Cron("0,20,40 * * * *").Zoned(TimeZoneInfo.Local).PreventOverlapping("Rcg3ReportScheduleLock");
                                scheduler.Schedule<Rcg3AuditSchedule>().EveryFifteenMinutes().PreventOverlapping("Rcg3AuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;
                        case nameof(Platform.EGSLOT):
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<EGSlotRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("EGSlotRecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<EGSlotRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("EGSlotRecordSummaryScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<EGSlotReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("EGSlotReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<EGSlotAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("EGSlotAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;
                        case nameof(Platform.CR):
                            if (openGame.Contains(Identity))
                            {
                                // 一般拉單排程
                                scheduler.Schedule<CRRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("CRRecordScheduleLock").RunOnceAtStart();
                                // 一般拉單例外處理排程
                                scheduler.Schedule<CRRecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("CRRecordFailoverScheduleLock");
                                // 遊戲注單後匯總排程(5分鐘彙總給H1的)
                                scheduler.Schedule<CRRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("CRRecordSummaryScheduleLock").RunOnceAtStart();
                                //// 遊戲注單報表排程(W1和遊戲商)
                                scheduler.Schedule<CRReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("CRReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                //// 遊戲注單補單排程
                                //scheduler.Schedule<CRAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("CRAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;
                        case nameof(Platform.WE):
                            if (openGame.Contains(Identity))
                            {
                                scheduler.Schedule<WERecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("WERecordScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<WERecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("WERecordSummaryScheduleLock").RunOnceAtStart();
                                scheduler.Schedule<WEReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("WEReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                scheduler.Schedule<WEAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("WEAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;

                        case nameof(Platform.IDN):
                            if (openGame.Contains(Identity))
                            {
                                // 一般拉單排程
                                scheduler.Schedule<IDNRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("IDNRecordScheduleLock").RunOnceAtStart();
                                // 一般拉單例外處理排程
                                scheduler.Schedule<IDNRecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("IDNRecordFailoverScheduleLock");
                                // 遊戲注單後匯總排程(5分鐘彙總給H1的)
                                scheduler.Schedule<IDNRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("IDNRecordSummaryScheduleLock").RunOnceAtStart();
                                // 遊戲注單報表排程(W1和遊戲商)
                                scheduler.Schedule<IDNReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("IDNReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                // 遊戲注單補單排程
                                scheduler.Schedule<IDNAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("IDNAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;
                        case nameof(Platform.PS):
                            if (openGame.Contains(Identity))
                            {
                                // 一般拉單排程
                                scheduler.Schedule<PSRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("PSRecordScheduleLock").RunOnceAtStart();
                                // 魚機拉單排程
                                //scheduler.Schedule<PSFishRecordSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("PSRecordFailoverScheduleLock");
                                // 遊戲注單後匯總排程(5分鐘彙總給H1的)
                                scheduler.Schedule<PSRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("PSRecordSummaryScheduleLock").RunOnceAtStart();
                                //// 遊戲注單報表排程(W1和遊戲商)
                                scheduler.Schedule<PSReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("PSReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                //// 遊戲注單補單排程
                                scheduler.Schedule<PSAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("PSAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;
                        case nameof(Platform.VA):
                            if (openGame.Contains(Identity))
                            {
                                // 一般拉單排程
                                scheduler.Schedule<VARecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("VARecordScheduleLock").RunOnceAtStart();
                                //// 一般拉單例外處理排程
                                scheduler.Schedule<VARecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("VARecordFailoverScheduleLock");
                                //// 遊戲注單後匯總排程(5分鐘彙總給H1的)
                                scheduler.Schedule<VARecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("VARecordSummaryScheduleLock").RunOnceAtStart();
                                //// 遊戲注單報表排程(W1和遊戲商)
                                scheduler.Schedule<VAReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("VAReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                //// 遊戲注單補單排程
                                scheduler.Schedule<VAAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("VAAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;
                        case nameof(Platform.SPLUS):
                            if (openGame.Contains(Identity))
                            {
                                // 一般拉單排程
                                scheduler.Schedule<SPLUSRecordSchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("SPLUSRecordScheduleLock").RunOnceAtStart();
                                // 一般拉單例外處理排程
                                scheduler.Schedule<SPLUSRecordFailoverSchedule>().EveryFiveSeconds().Zoned(TimeZoneInfo.Local).PreventOverlapping("SPLUSRecordFailoverScheduleLock");
                                // 遊戲注單後匯總排程(5分鐘彙總給H1的)
                                scheduler.Schedule<SPLUSRecordSummarySchedule>().EverySeconds(20).Zoned(TimeZoneInfo.Local).PreventOverlapping("SPLUSRecordSummaryScheduleLock").RunOnceAtStart();
                                // 遊戲注單報表排程(W1和遊戲商)
                                scheduler.Schedule<SPLUSReportSchedule>().Cron("0,20,40 * * * *").PreventOverlapping("SPLUSReportScheduleLock").Zoned(TimeZoneInfo.Local);
                                // 遊戲注單補單排程
                                scheduler.Schedule<SPLUSAuditSchedule>().Cron("10,30,50 * * * *").PreventOverlapping("SPLUSAuditScheduleLock").Zoned(TimeZoneInfo.Local);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }).LogScheduledTaskProgress(app.ApplicationServices.GetRequiredService<ILogger<Scheduler>>());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}