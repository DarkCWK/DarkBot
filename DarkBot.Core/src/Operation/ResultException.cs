using System;

namespace DarkBot.Core.Operation;

public class ResultException(int code) : Exception($"result fail: {code}") {
    public int Code { get; } = code;
}