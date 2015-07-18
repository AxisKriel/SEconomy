/*
 * Jist 1 compatibility shim for jist 2.0
 * 
 * Copyright (C) Tyler Watson, 2015
 */


seconomy_transfer_async = function (from, to, amount, msg, cb) {
    seconomy.TransferAsync(from, to, amount, msg, cb);
}

seconomy_pay_async = function (from, to, amount, msg, cb) {
    seconomy.PayAsync(from, to, amount, msg, cb);
}

seconomy_parse_money = function (money) {
    return seconomy.ParseMoney(money);
}

seconomy_valid_money = function (money) {
    return seconomy.MoneyValid(money);
}

seconomy_get_account = function (account) {
    return seconomy.GetBankAccount(account);
}

seconomy_set_multiplier = function (multi) {
    seconomy.SetMultiplier(multi);
}

seconomy_get_multiplier = function () {
    return seconomy.GetMultiplier();
}

seconomy_world_account = function () {
    return seconomy.WorldAccount();
}