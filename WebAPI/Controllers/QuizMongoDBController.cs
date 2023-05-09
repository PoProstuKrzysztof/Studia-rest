using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Infrastructure.MongoDB.Entities;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;

namespace WebAPI.Controllers;

[ApiController, Route( "/api/v2/mongodb" )]
public class QuizMongoDBController : ControllerBase
{
    private readonly QuizUserServiceMongoDB _service;

    public QuizMongoDBController(QuizUserServiceMongoDB service)
    {
        _service = service;
    }

    [HttpGet( "/quidId" )]
    public ActionResult<Quiz> GetQuiz(int quizId)
    {
        var quiz = _service.FindQuizById( quizId );
        if (quiz == null)
        {
            return NotFound();
        }
        return quiz;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Quiz>> GetAllQuiz()
    {
        return Ok( _service.FindAllQuizzess() );
    }

    [HttpPost]
    [Route( "{quizId}/items/{itemId}" )]
    public void SaveAnswer([FromBody] QuizItemAnswerDto dto, [FromRoute] int quizId,
    [FromRoute] int itemId)
    {
        _service.SaveUserAnswerForQuiz( quizId, dto.UserId, itemId, dto.Answer );
    }

    [HttpGet]
    [Route( "/{userId}/quizesAnswers/{quizId}/mongoDB" )]
    public QuizUserAnswerDto GetCorrectAnswers([FromRoute] int userId, [FromRoute] int quizId)
    {
        var answers = _service.GetUserAnswersForQuiz( quizId, userId );

        return new QuizUserAnswerDto()
        {
            QuizId = quizId,
            UserId = 1,
            QuizAnswers = answers.Select( x => new QuizItemUserAnswersDto()
            {
                Question = x.QuizItem.Question,
                Answer = x.Answer,
                IsCorrect = x.IsCorrect(),
                QuizItemId = 1
            } ).ToList()
        };
    }
}