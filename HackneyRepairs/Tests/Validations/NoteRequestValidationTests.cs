using System;
using Bogus;
using HackneyRepairs.Models;
using HackneyRepairs.Validators;
using Xunit;

namespace HackneyRepairs.Tests.Validations
{
    public class NoteRequestValidationTests
    {
        NoteRequest request;
        Faker fakeRandomValue;
        NoteRequestValidator requestValidator;

        public NoteRequestValidationTests()
        {
            requestValidator = new NoteRequestValidator();
            fakeRandomValue = new Faker();
            request = new NoteRequest
            {
                ObjectKey = "uhorder",
                Text = "This text is under 2000 chars",
                ObjectReference = "12345678",
                LBHEmail = "bob@hackney.gov.uk"            
            };
        }

        [Fact]
        public void return_true_if_noterequest_is_valid()
        {
            var result = requestValidator.Validate(request);
            Assert.True(result.Valid);
            Assert.Empty(result.ErrorMessages);
        }

        [Fact]
        public void return_false_if_noterequest_is_invalid_object()
        {
            NoteRequest badRequest = null;
            var result = requestValidator.Validate(badRequest);
            Assert.False(result.Valid);
            Assert.Single(result.ErrorMessages);
        }

        [Fact]
        public void return_false_if_Objectkey_is_other_than_uhorder()
        {
            request.ObjectKey = fakeRandomValue.Lorem.Word();
            var result = requestValidator.Validate(request);

            Assert.False(result.Valid);
            Assert.Single(result.ErrorMessages);
        }

        [Fact]
        public void return_false_if_text_is_over_2000_chars()
        {
            request.Text = fakeRandomValue.Lorem.Sentence(1500);
            var result = requestValidator.Validate(request);

            Assert.False(result.Valid);
            Assert.Single(result.ErrorMessages);
        }

        [Fact]
        public void return_false_with_multiple_errors_if_multiple_attributes_are_invalid()
        {
            var invalidRequest = new NoteRequest
            {
                ObjectKey = fakeRandomValue.Lorem.Word(),
                Text = "",
                ObjectReference = null,
                LBHEmail = "bob@hackeny.gov.uk"             
            };
            var result = requestValidator.Validate(invalidRequest);

            Assert.False(result.Valid);
            Assert.Equal(3, result.ErrorMessages.Count);
        }
    }
}
