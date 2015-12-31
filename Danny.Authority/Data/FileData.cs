using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Danny.Authority.Data
{
    /**
     * @ 文件实体类
     * */
    public class FileData
    {
        /**
         * @ 文件编号，主键
         * */
        public string ID { get; set; }
        /**
         * @ 文件名称
         * */
        public string Name { get; set; }
        /**
         * @ 文件路径
         * */
        public string Url { get; set; }
    }
}
