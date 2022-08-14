using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharewinfo.flow.Controller
{
    /// <summary>
    /// 流程引擎 控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FlowCoreController: ControllerBase
    {
        [HttpPost("SaveFlow/{flowId}")]
        public ActionResult<dynamic> SaveLamsPrice(int flowId)
        {
            return Ok(flowId);
        }
     }
}
