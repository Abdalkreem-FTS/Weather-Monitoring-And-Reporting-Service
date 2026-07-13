using System.Text.Json;

namespace WMARS.Tests.Results;

public class ResultTests
{
    [Fact]
    public void From_Value_Creates_Success()
    {
        var result = Result<string>.From("hello");

        result.IsSuccess.Should().BeTrue();
        result.IsError.Should().BeFalse();
    }

    [Fact]
    public void Implicit_Conversion_From_Value_Creates_Success()
    {
        Result<string> result = "implicit";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("implicit");
    }

    [Fact]
    public void Successful_Result_Exposes_Value_And_No_Errors()
    {
        var result = Result<string>.From("hello");

        result.Value.Should().Be("hello");
        result.Errors.Should().NotBeNull().And.BeEmpty();
        result.TopError.Should().Be(default(Error));
    }

    [Fact]
    public void Error_Result_Exposes_Errors_And_Default_Value()
    {
        Result<string> result = Error.NotFound("File.NotFound", "nope");

        result.IsError.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().ContainSingle();
        result.TopError.Code.Should().Be("File.NotFound");
    }

    [Fact]
    public void TopError_Returns_First_Error_When_Multiple()
    {
        List<Error> errors = [Error.Validation("first", "1"), Error.Failure("second", "2")];

        Result<string> result = errors;

        result.Errors.Should().HaveCount(2);
        result.TopError.Code.Should().Be("first");
    }

    [Fact]
    public void Constructing_From_Empty_Error_List_Falls_Back_To_EmptyErrors()
    {
        List<Error> empty = [];

        Result<string> result = empty;

        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("Result.EmptyErrors");
    }

    [Fact]
    public void From_Null_Value_Returns_NullValue_Error()
    {
        var result = Result<string>.From(null!);

        result.IsError.Should().BeTrue();
        result.TopError.Code.Should().Be("Result.NullValue");
    }

    [Fact]
    public void Match_On_Success_Invokes_OnValue()
    {
        var result = Result<string>.From("hi");

        var output = result.Match(value => $"ok:{value}", errors => $"err:{errors.Count}");

        output.Should().Be("ok:hi");
    }

    [Fact]
    public void Match_On_Error_Invokes_OnError()
    {
        Result<string> result = Error.Failure("E", "boom");

        var output = result.Match(value => $"ok:{value}", errors => $"err:{errors.Count}");

        output.Should().Be("err:1");
    }

    [Fact]
    public void Deserialize_Successful_Result_Restores_Value()
    {
        const string json = """{ "Value": "restored", "IsSuccess": true, "Errors": null }""";

        var result = JsonSerializer.Deserialize<Result<string>>(json);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("restored");
        result.Errors.Should().BeEmpty();
        result.TopError.Should().Be(default(Error));
    }

    [Fact]
    public void Deserialize_Success_Flag_With_Null_Value_Becomes_Error()
    {
        const string json = """{ "Value": null, "IsSuccess": true, "Errors": null }""";

        var result = JsonSerializer.Deserialize<Result<string>>(json);

        result!.IsError.Should().BeTrue();
        result.TopError.Code.Should().Be("Result.NullValue");
    }

    [Fact]
    public void Deserialize_Failed_Result_Without_Errors_Uses_Fallback_Error()
    {
        const string json = """{ "Value": null, "IsSuccess": false, "Errors": null }""";

        var result = JsonSerializer.Deserialize<Result<string>>(json);

        result!.IsError.Should().BeTrue();
        result.TopError.Code.Should().Be("Result.EmptyErrors");
    }
}
