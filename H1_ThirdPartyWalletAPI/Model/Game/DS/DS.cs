using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS
{
    public class DS
    {
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "en_us"},
            {"th-TH", "th_th"},
            {"vi-VN", "vi_vn"},
            {"zh-TW", "zh_hk"},
            {"zh-CN", "zh_cn"},
            {"id-ID", "id_id"},
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB", "THB"},
        };
    }
    public enum error_code
    {
        unknown_error = 0,
        succeeded, 
        duplicated,
        required_field,
        login_failed,
        API_access_failed,
        information_not_found,
        request_time_out,
        verification_code_invalid,
        user_blocked,
        player_cannot_be_found,
        agent_cannot_be_found,
        content_type_error,
        signature_failed = 1000,
	    amount_error,
        transaction_number_duplicated,
        verification_failed,
        data_base_access_failed,
        wallet_not_found,
        transaction_not_found,
        processing_failed,
        transaction_amount_cannot_be_negative_number,        
        agent_amount_not_found,
        agent_amount_insufficient,
        agent_withdrawal_refused,
        time_is_out_of_range,
        exceed_the_second_decimal_place,
        game_has_not_been_settled = 1028,
        trnasfer_response_format_fail,

    }
}
