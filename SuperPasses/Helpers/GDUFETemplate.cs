namespace SuperPasses.Helpers;
// 广东财经大学校园网
public class GDUFETemplate
{
    private string _certificationURL = @"http://100.64.13.17:801/eportal/portal/login?callback=dr1003&login_method=1";

    private readonly string _urlSuffix = @"&wlan_ac_ip=100.64.13.18&wlan_ac_name=&jsVersion=4.1.3&terminal_type=1&lang=zh-cn&v=502&lang=zh";
    private readonly string _param1 = @" -H 'Accept: */*'";
    private readonly string _param2 = @" -H 'Accept-Language: zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6'";
    private readonly string _param3 = @" -H 'Connection: keep-alive'";
    private readonly string _param4 = @" -H 'Referer: http://100.64.13.17/'";
    private readonly string _param5 = @" -H 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36 Edg/110.0.1587.57'";


    public string GetCertificationRequest(string account, string password, string mac, string ip)
    {
        return
            $"'{_certificationURL}&user_account=%2C0%2C{account}&user_password={password}&wlan_user_ip={ip}&wlan_user_ipv6=&wlan_user_mac={mac}{_urlSuffix}'{_param1}{_param2}{_param3}{_param4}{_param5}";
    }
    
    // 不超过512个字节
    public string GetCertificationRequestShort(string account, string password, string mac, string ip)
    {
        return
            $"'{_certificationURL}&user_account=%2C0%2C{account}&user_password={password}&wlan_user_ip={ip}&wlan_user_ipv6=&wlan_user_mac={mac}{_urlSuffix}'{_param1}{_param2}{_param3}{_param4}";
    }
}