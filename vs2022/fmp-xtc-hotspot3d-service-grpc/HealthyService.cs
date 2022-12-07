
using Grpc.Core;
using System.Threading.Tasks;
using XTC.FMP.MOD.Hotspot3D.LIB.Proto;

namespace XTC.FMP.MOD.Hotspot3D.App.Service
{
    public class HealthyService : HealthyServiceBase
    {
        private readonly SingletonServices singletonServices_;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <remarks>
        /// 支持多个参数，均为自动注入，注入点位于MyProgram.PreBuild
        /// </remarks>
        /// <param name="_singletonServices">自动注入的单例服务</param>
        public HealthyService(SingletonServices _singletonServices)
        {
            singletonServices_ = _singletonServices;
        }
    }
}
