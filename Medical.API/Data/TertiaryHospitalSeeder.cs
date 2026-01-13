using Medical.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Medical.API.Data;

/// <summary>
/// 三甲医院数据种子
/// </summary>
public static class TertiaryHospitalSeeder
{
    /// <summary>
    /// 初始化三甲医院数据
    /// </summary>
    public static async Task SeedAsync(MedicalDbContext context)
    {
        // 检查是否已有数据
        if (await context.TertiaryHospitals.AnyAsync())
        {
            return; // 已有数据，跳过
        }

        // 获取所有省份和城市数据
        var provinces = await context.Provinces.ToListAsync();
        var cities = await context.Cities.Include(c => c.Province).ToListAsync();

        // 创建省份和城市名称到ID的映射
        var provinceMap = provinces.ToDictionary(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase);
        var cityMap = cities.ToDictionary(c => $"{c.Province.Name}_{c.Name}", c => c.Id, StringComparer.OrdinalIgnoreCase);

        // 辅助函数：根据省份和城市名称获取ID
        Guid? GetProvinceId(string provinceName)
        {
            // 处理省份名称的映射
            var normalizedName = provinceName switch
            {
                "北京" => "北京市",
                "上海" => "上海市",
                "天津" => "天津市",
                "重庆" => "重庆市",
                "广东" => "广东省",
                "广西" => "广西壮族自治区",
                "新疆" => "新疆维吾尔自治区",
                "内蒙古" => "内蒙古自治区",
                "西藏" => "西藏自治区",
                "宁夏" => "宁夏回族自治区",
                _ => provinceName.EndsWith("省") || provinceName.EndsWith("市") || provinceName.EndsWith("自治区") 
                    ? provinceName 
                    : provinceName + "省"
            };
            return provinceMap.TryGetValue(normalizedName, out var id) ? id : null;
        }

        Guid? GetCityId(string provinceName, string cityName)
        {
            var normalizedProvinceName = provinceName switch
            {
                "北京" => "北京市",
                "上海" => "上海市",
                "天津" => "天津市",
                "重庆" => "重庆市",
                "广东" => "广东省",
                "广西" => "广西壮族自治区",
                "新疆" => "新疆维吾尔自治区",
                "内蒙古" => "内蒙古自治区",
                "西藏" => "西藏自治区",
                "宁夏" => "宁夏回族自治区",
                _ => provinceName.EndsWith("省") || provinceName.EndsWith("市") || provinceName.EndsWith("自治区") 
                    ? provinceName 
                    : provinceName + "省"
            };
            
            // 对于直辖市，城市名称实际上是区名，需要特殊处理
            if (normalizedProvinceName == "北京市" || normalizedProvinceName == "上海市" || 
                normalizedProvinceName == "天津市" || normalizedProvinceName == "重庆市")
            {
                // 对于直辖市，如果城市名称和省份名称相同（如"北京"），使用第一个城市（通常是主城区）
                if (cityName == provinceName)
                {
                    var firstCity = cities.FirstOrDefault(c => c.Province.Name == normalizedProvinceName);
                    return firstCity?.Id;
                }
                
                // 否则尝试匹配区名
                var matchingCity = cities.FirstOrDefault(c => 
                    c.Province.Name == normalizedProvinceName && 
                    (c.Name == cityName || c.Name.Contains(cityName) || cityName.Contains(c.Name.Replace("区", ""))));
                
                return matchingCity?.Id;
            }
            
            // 处理普通省份的城市名称（添加"市"后缀）
            var normalizedCityName = cityName.EndsWith("市") ? cityName : cityName + "市";
            
            var key = $"{normalizedProvinceName}_{normalizedCityName}";
            if (cityMap.TryGetValue(key, out var id))
            {
                return id;
            }
            
            // 如果精确匹配失败，尝试模糊匹配
            var matchingCity2 = cities.FirstOrDefault(c => 
                c.Province.Name == normalizedProvinceName && 
                (c.Name == normalizedCityName || c.Name == cityName || c.Name.Contains(cityName) || cityName.Contains(c.Name.Replace("市", ""))));
            
            return matchingCity2?.Id;
        }

        // 定义医院数据（省份名称，城市名称，医院名称，地址，类型，电话，官网）
        var hospitalData = new List<(string province, string city, string name, string address, string type, string phone, string website)>
        {
            // 北京
            ("北京", "北京", "北京协和医院", "东城区东单北大街53号", "综合医院", "010-69156114", "https://www.pumch.cn"),
            ("北京", "北京", "中国人民解放军总医院（301医院）", "海淀区复兴路28号", "综合医院", "010-66936182", "https://www.301hospital.com.cn"),
            ("北京", "北京", "北京大学第一医院", "西城区西什库大街8号", "综合医院", "010-66551056", "https://www.bddyyy.com.cn"),
            ("北京", "北京", "北京大学人民医院", "西城区西直门南大街11号", "综合医院", "010-88326666", "https://www.pkuph.cn"),
            ("北京", "北京", "北京天坛医院", "丰台区南四环西路119号", "神经外科", "010-59976518", "https://www.bjtth.org"),
            ("北京", "北京", "北京阜外医院", "西城区北礼士路167号", "心血管", "010-88398866", "https://www.fuwai.com"),
            ("北京", "北京", "首都医科大学附属北京朝阳医院", "朝阳区工人体育场南路8号", "综合医院", "010-85231000", "https://www.bjcyh.com.cn"),
            ("北京", "北京", "首都医科大学附属北京安贞医院", "朝阳区安贞路2号", "综合医院", "010-64412431", "https://www.anzhen.org"),
            ("北京", "北京", "首都医科大学附属北京同仁医院", "东城区东交民巷1号", "综合医院", "010-58269911", "https://www.trhos.com"),
            ("北京", "北京", "首都医科大学附属北京儿童医院", "西城区南礼士路56号", "儿科", "010-59616161", "https://www.bch.com.cn"),

            // 上海
            ("上海", "上海", "复旦大学附属华山医院", "静安区乌鲁木齐中路12号", "综合医院", "021-62489999", "https://www.huashan.org.cn"),
            ("上海", "上海", "上海交通大学医学院附属瑞金医院", "黄浦区瑞金二路197号", "综合医院", "021-64370045", "https://www.rjh.com.cn"),
            ("上海", "上海", "复旦大学附属中山医院", "徐汇区枫林路180号", "综合医院", "021-64041990", "https://www.zs-hospital.sh.cn"),
            ("上海", "上海", "上海交通大学医学院附属仁济医院", "黄浦区山东中路145号", "综合医院", "021-58752345", "https://www.renji.com"),
            ("上海", "上海", "上海交通大学医学院附属第九人民医院", "黄浦区制造局路639号", "综合医院", "021-23271699", "https://www.9hospital.com.cn"),
            ("上海", "上海", "上海交通大学医学院附属新华医院", "杨浦区控江路1665号", "综合医院", "021-25078999", "https://www.xinhua.org.cn"),
            ("上海", "上海", "上海市第一人民医院", "虹口区武进路85号", "综合医院", "021-63240090", "https://www.firsthospital.cn"),
            ("上海", "上海", "上海市第六人民医院", "徐汇区宜山路600号", "综合医院", "021-64369181", "https://www.6thhosp.com"),
            ("上海", "上海", "上海长海医院", "杨浦区长海路168号", "综合医院", "021-31161818", "https://www.chhospital.com.cn"),
            ("上海", "上海", "上海东方医院", "浦东新区即墨路150号", "综合医院", "021-38804518", "https://www.easthospital.cn"),

            // 广东
            ("广东", "广州", "中山大学附属第一医院", "越秀区中山二路58号", "综合医院", "020-87755766", "https://www.gzsums.net"),
            ("广东", "广州", "南方医科大学南方医院", "白云区广州大道北1838号", "综合医院", "020-61641888", "https://www.nfyy.com"),
            ("广东", "广州", "广东省人民医院", "越秀区中山二路106号", "综合医院", "020-83827812", "https://www.gdghospital.org.cn"),
            ("广东", "广州", "广州医科大学附属第一医院", "越秀区沿江路151号", "综合医院", "020-83062114", "https://www.gyfyy.com"),
            ("广东", "广州", "中山大学肿瘤防治中心", "越秀区东风东路651号", "肿瘤", "020-87343088", "https://www.sysucc.org.cn"),
            ("广东", "深圳", "深圳市人民医院", "罗湖区东门北路1017号", "综合医院", "0755-25533018", "https://www.szrmyy.com"),
            ("广东", "深圳", "深圳市第二人民医院", "福田区笋岗路3002号", "综合医院", "0755-83366388", "https://www.szrch.com"),
            ("广东", "深圳", "北京大学深圳医院", "福田区莲花路1120号", "综合医院", "0755-83923333", "https://www.szpkuh.com"),
            ("广东", "佛山", "佛山市第一人民医院", "禅城区岭南大道北81号", "综合医院", "0757-83163888", "https://www.fsyyy.com"),
            ("广东", "东莞", "东莞市人民医院", "万江区新谷涌万道路南3号", "综合医院", "0769-28633333", "https://www.dgphospital.com"),

            // 浙江
            ("浙江", "杭州", "浙江大学医学院附属第一医院", "上城区庆春路79号", "综合医院", "0571-87236114", "https://www.zy91.com"),
            ("浙江", "杭州", "浙江大学医学院附属第二医院", "上城区解放路88号", "综合医院", "0571-87783777", "https://www.z2hospital.com"),
            ("浙江", "杭州", "浙江大学医学院附属邵逸夫医院", "江干区庆春东路3号", "综合医院", "0571-86090073", "https://www.srrsh.com"),
            ("浙江", "杭州", "浙江省人民医院", "拱墅区上塘路158号", "综合医院", "0571-85893000", "https://www.hospitalstar.com"),
            ("浙江", "温州", "温州医科大学附属第一医院", "鹿城区府学巷2号", "综合医院", "0577-88069300", "https://www.wzhospital.cn"),
            ("浙江", "宁波", "宁波市第一医院", "海曙区柳汀街59号", "综合医院", "0574-87085555", "https://www.nbdyyy.com"),

            // 江苏
            ("江苏", "南京", "江苏省人民医院", "鼓楼区广州路300号", "综合医院", "025-83718836", "https://www.jsph.net.cn"),
            ("江苏", "南京", "南京鼓楼医院", "鼓楼区中山路321号", "综合医院", "025-83106666", "https://www.njglyy.com"),
            ("江苏", "苏州", "苏州大学附属第一医院", "姑苏区十梓街188号", "综合医院", "0512-65223637", "https://www.sdfyy.cn"),
            ("江苏", "无锡", "无锡市人民医院", "梁溪区清扬路299号", "综合医院", "0510-85351888", "https://www.wuxiph.com"),
            ("江苏", "常州", "常州市第一人民医院", "天宁区局前街185号", "综合医院", "0519-68870000", "https://www.czfph.com"),

            // 山东
            ("山东", "济南", "山东大学齐鲁医院", "历下区文化西路107号", "综合医院", "0531-82169114", "https://www.qiluhospital.com"),
            ("山东", "济南", "山东省立医院", "槐荫区经五路324号", "综合医院", "0531-87938911", "https://www.sph.com.cn"),
            ("山东", "青岛", "青岛大学附属医院", "市南区江苏路16号", "综合医院", "0532-82912222", "https://www.qduh.cn"),
            ("山东", "烟台", "烟台毓璜顶医院", "芝罘区毓璜顶东路20号", "综合医院", "0535-6691999", "https://www.ytyhdyy.com"),

            // 四川
            ("四川", "成都", "四川大学华西医院", "武侯区国学巷37号", "综合医院", "028-85422286", "https://www.cd120.com"),
            ("四川", "成都", "四川省人民医院", "青羊区一环路西二段32号", "综合医院", "028-87393999", "https://www.samsph.com"),
            ("四川", "成都", "成都军区总医院", "金牛区蓉都大道270号", "综合医院", "028-86570114", "https://www.cdjqzyy.com"),

            // 湖北
            ("湖北", "武汉", "华中科技大学同济医学院附属同济医院", "硚口区解放大道1095号", "综合医院", "027-83662688", "https://www.tjh.com.cn"),
            ("湖北", "武汉", "华中科技大学同济医学院附属协和医院", "江汉区解放大道1277号", "综合医院", "027-85351688", "https://www.whuh.com"),
            ("湖北", "武汉", "武汉大学人民医院", "武昌区张之洞路99号", "综合医院", "027-88041911", "https://www.rmhospital.com"),
            ("湖北", "武汉", "武汉大学中南医院", "武昌区东湖路169号", "综合医院", "027-67812888", "https://www.znhospital.cn"),

            // 河南
            ("河南", "郑州", "郑州大学第一附属医院", "二七区建设东路1号", "综合医院", "0371-66913114", "https://www.fccmu.com"),
            ("河南", "郑州", "河南省人民医院", "金水区纬五路7号", "综合医院", "0371-65580014", "https://www.hnsrmyy.net"),
            ("河南", "洛阳", "河南科技大学第一附属医院", "涧西区景华路24号", "综合医院", "0379-64830600", "https://www.lyhospital.com"),

            // 湖南
            ("湖南", "长沙", "中南大学湘雅医院", "开福区湘雅路87号", "综合医院", "0731-84328888", "https://www.xiangya.com.cn"),
            ("湖南", "长沙", "中南大学湘雅二医院", "芙蓉区人民中路139号", "综合医院", "0731-85295114", "https://www.xyeyy.com"),
            ("湖南", "长沙", "湖南省人民医院", "芙蓉区解放西路61号", "综合医院", "0731-82278000", "https://www.hnsrmyy.com"),

            // 福建
            ("福建", "福州", "福建医科大学附属协和医院", "鼓楼区新权路29号", "综合医院", "0591-83357896", "https://www.fjxiehe.com"),
            ("福建", "福州", "福建医科大学附属第一医院", "台江区茶中路20号", "综合医院", "0591-87983333", "https://www.fyyy.com"),
            ("福建", "厦门", "厦门大学附属第一医院", "思明区镇海路55号", "综合医院", "0592-2132222", "https://www.xmfh.com.cn"),

            // 安徽
            ("安徽", "合肥", "安徽医科大学第一附属医院", "蜀山区绩溪路218号", "综合医院", "0551-62922114", "https://www.ayfy.com"),
            ("安徽", "合肥", "安徽省立医院", "庐阳区庐江路17号", "综合医院", "0551-62283114", "https://www.ahslyy.com.cn"),

            // 辽宁
            ("辽宁", "沈阳", "中国医科大学附属第一医院", "和平区南京北街155号", "综合医院", "024-83283333", "https://www.cmu1h.com"),
            ("辽宁", "沈阳", "中国医科大学附属盛京医院", "和平区三好街36号", "综合医院", "024-96615", "https://www.sj-hospital.org"),
            ("辽宁", "大连", "大连医科大学附属第一医院", "沙河口区中山路222号", "综合医院", "0411-83635963", "https://www.dmu-1.com"),

            // 河北
            ("河北", "石家庄", "河北医科大学第二医院", "新华区和平西路215号", "综合医院", "0311-66002120", "https://www.hb2h.com"),
            ("河北", "石家庄", "河北医科大学第四医院", "长安区健康路12号", "综合医院", "0311-86095200", "https://www.hb4h.com"),

            // 山西
            ("山西", "太原", "山西医科大学第一医院", "迎泽区解放南路85号", "综合医院", "0351-4639114", "https://www.sydyy.com.cn"),
            ("山西", "太原", "山西省人民医院", "迎泽区双塔寺街29号", "综合医院", "0351-4960131", "https://www.sxsrmyy.com"),

            // 吉林
            ("吉林", "长春", "吉林大学第一医院", "朝阳区新民大街1号", "综合医院", "0431-88782222", "https://www.jdyy.cn"),
            ("吉林", "长春", "吉林大学第二医院", "南关区自强街218号", "综合医院", "0431-88796222", "https://www.jdey.com.cn"),

            // 黑龙江
            ("黑龙江", "哈尔滨", "哈尔滨医科大学附属第一医院", "南岗区邮政街23号", "综合医院", "0451-53643849", "https://www.hrbmu.edu.cn"),
            ("黑龙江", "哈尔滨", "哈尔滨医科大学附属第二医院", "南岗区学府路246号", "综合医院", "0451-86605222", "https://www.hrbmush.edu.cn"),

            // 江西
            ("江西", "南昌", "南昌大学第一附属医院", "东湖区永外正街17号", "综合医院", "0791-88692500", "https://www.cdyfy.com"),
            ("江西", "南昌", "江西省人民医院", "东湖区爱国路152号", "综合医院", "0791-86895511", "https://www.jxsrmyy.cn"),

            // 重庆
            ("重庆", "重庆", "重庆医科大学附属第一医院", "渝中区袁家岗友谊路1号", "综合医院", "023-68811360", "https://www.hospital-cqmu.com"),
            ("重庆", "重庆", "第三军医大学西南医院", "沙坪坝区高滩岩正街30号", "综合医院", "023-68754000", "https://www.xnyy.cn"),
            ("重庆", "重庆", "重庆医科大学附属第二医院", "渝中区临江路74号", "综合医院", "023-63693222", "https://www.cqmu.edu.cn"),

            // 天津
            ("天津", "天津", "天津医科大学总医院", "和平区鞍山道154号", "综合医院", "022-60362222", "https://www.tjmugh.com.cn"),
            ("天津", "天津", "天津市第一中心医院", "南开区复康路24号", "综合医院", "022-23626000", "https://www.tjzxh.com"),
            ("天津", "天津", "天津医科大学第二医院", "河西区平江道23号", "综合医院", "022-88328000", "https://www.tjmu2h.com.cn"),

            // 云南
            ("云南", "昆明", "昆明医科大学第一附属医院", "五华区西昌路295号", "综合医院", "0871-65324888", "https://www.ydyy.cn"),
            ("云南", "昆明", "云南省第一人民医院", "西山区金碧路157号", "综合医院", "0871-63638273", "https://www.ynsdyrmyy.com"),

            // 广西
            ("广西", "南宁", "广西医科大学第一附属医院", "青秀区双拥路6号", "综合医院", "0771-5356501", "https://www.gxmu.edu.cn"),
            ("广西", "南宁", "广西壮族自治区人民医院", "青秀区桃源路6号", "综合医院", "0771-2186000", "https://www.gxhospital.com"),

            // 新疆
            ("新疆", "乌鲁木齐", "新疆医科大学第一附属医院", "新市区鲤鱼山南路137号", "综合医院", "0991-4362931", "https://www.xjmu.edu.cn"),
            ("新疆", "乌鲁木齐", "新疆维吾尔自治区人民医院", "天山区天池路91号", "综合医院", "0991-8562200", "https://www.xjrmyy.com"),

            // 内蒙古
            ("内蒙古", "呼和浩特", "内蒙古医科大学附属医院", "回民区通道北街1号", "综合医院", "0471-3451021", "https://www.nmgfy.com"),
            ("内蒙古", "呼和浩特", "内蒙古自治区人民医院", "赛罕区昭乌达路20号", "综合医院", "0471-6620000", "https://www.nmgyy.cn"),

            // 西藏
            ("西藏", "拉萨", "西藏自治区人民医院", "城关区林廓北路18号", "综合医院", "0891-6371462", "https://www.xzrmyy.com"),

            // 宁夏
            ("宁夏", "银川", "宁夏医科大学总医院", "兴庆区胜利街804号", "综合医院", "0951-6744524", "https://www.nyfy.com.cn"),
            ("宁夏", "银川", "宁夏回族自治区人民医院", "金凤区正源北街301号", "综合医院", "0951-5920000", "https://www.nxrmyy.com"),

            // 青海
            ("青海", "西宁", "青海大学附属医院", "城西区同仁路29号", "综合医院", "0971-6162000", "https://www.qhu.edu.cn"),
            ("青海", "西宁", "青海省人民医院", "城东区共和路2号", "综合医院", "0971-8177911", "https://www.qhrmyy.com"),

            // 甘肃
            ("甘肃", "兰州", "兰州大学第一医院", "城关区东岗西路1号", "综合医院", "0931-8625000", "https://www.ldyy.cn"),
            ("甘肃", "兰州", "兰州大学第二医院", "城关区萃英门82号", "综合医院", "0931-8942266", "https://www.ldey.com.cn"),
            ("甘肃", "兰州", "甘肃省人民医院", "城关区东岗西路204号", "综合医院", "0931-8281114", "https://www.gsrmyy.com"),

            // 贵州
            ("贵州", "贵阳", "贵州医科大学附属医院", "云岩区贵医街28号", "综合医院", "0851-86772233", "https://www.gmcah.com"),
            ("贵州", "贵阳", "贵州省人民医院", "南明区中山东路83号", "综合医院", "0851-85922999", "https://www.gz5055.com"),

            // 海南
            ("海南", "海口", "海南医学院第一附属医院", "龙华区龙华路31号", "综合医院", "0898-66772248", "https://www.hnmu.edu.cn"),
            ("海南", "海口", "海南省人民医院", "龙华区秀英区秀华路19号", "综合医院", "0898-68642548", "https://www.hnph.com.cn"),

            // 陕西
            ("陕西", "西安", "西安交通大学第一附属医院", "雁塔区雁塔西路277号", "综合医院", "029-85323888", "https://www.dyyy.xjtu.edu.cn"),
            ("陕西", "西安", "西安交通大学第二附属医院", "新城区西五路157号", "综合医院", "029-87679322", "https://www.2yuan.xjtu.edu.cn"),
            ("陕西", "西安", "第四军医大学西京医院", "新城区长乐西路127号", "综合医院", "029-84775511", "https://www.fmmu.edu.cn"),
            ("陕西", "西安", "陕西省人民医院", "碑林区友谊西路256号", "综合医院", "029-85251331", "https://www.spph-sx.com")
        };

        // 地理编码函数：将地址转换为经纬度
        async Task<(decimal? latitude, decimal? longitude)> GeocodeAddressAsync(string fullAddress)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "MedicalApp/1.0");
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var encodedAddress = Uri.EscapeDataString(fullAddress);
                var url = $"https://nominatim.openstreetmap.org/search?format=json&q={encodedAddress}&limit=1&accept-language=zh-CN";

                var response = await httpClient.GetStringAsync(url);
                var results = JsonSerializer.Deserialize<JsonElement[]>(response);

                if (results != null && results.Length > 0)
                {
                    var lat = results[0].GetProperty("lat").GetString();
                    var lon = results[0].GetProperty("lon").GetString();
                    if (decimal.TryParse(lat, out var latitude) && decimal.TryParse(lon, out var longitude))
                    {
                        return (latitude, longitude);
                    }
                }
            }
            catch (Exception ex)
            {
                // 静默失败，返回null
                Console.WriteLine($"地理编码失败: {fullAddress}, 错误: {ex.Message}");
            }

            return (null, null);
        }

        var hospitals = new List<TertiaryHospital>();
        int sortOrder = 1;

        foreach (var (province, city, name, address, type, phone, website) in hospitalData)
        {
            var provinceId = GetProvinceId(province);
            var cityId = GetCityId(province, city);

            if (provinceId.HasValue && cityId.HasValue)
            {
                // 构建完整地址进行地理编码
                var fullAddress = $"{province}{city}{address}{name}";
                var (latitude, longitude) = await GeocodeAddressAsync(fullAddress);

                hospitals.Add(new TertiaryHospital
                {
                    Name = name,
                    ProvinceId = provinceId.Value,
                    CityId = cityId.Value,
                    Address = address,
                    Level = "三甲",
                    Type = type,
                    Phone = phone,
                    Website = website,
                    Latitude = latitude,
                    Longitude = longitude,
                    SortOrder = sortOrder++,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                // 添加延迟以避免API限流（每1秒一个请求）
                await Task.Delay(1000);
            }
            else
            {
                // 记录无法匹配的医院（可选：记录日志）
                Console.WriteLine($"警告：无法找到省份或城市 - 医院: {name}, 省份: {province}, 城市: {city}");
            }
        }

        if (hospitals.Count > 0)
        {
            await context.TertiaryHospitals.AddRangeAsync(hospitals);
            await context.SaveChangesAsync();
        }
    }
}
