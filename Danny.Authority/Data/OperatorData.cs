using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Danny.Authority.Data
{
    /**
     * @ 模块操作实体类
     * */
    public class OperatorData
    {
        /**
         * @ 操作编号，主键
         * */
        public string ID { get; set; }
        /**
         * @ 操作名称
         * */
        public string Name { get; set; }
        /**
         * @ 所属领域
         * */
        public string Domain { get; set; }
        /**
         * @ 操作编码
         * */
        public string Code { get; set; }
    }
}
