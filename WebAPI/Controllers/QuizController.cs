using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Infrastructure.EF.Entities;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using WebAPI.Configuration;
using WebAPI.DTO;

namespace WebAPI.Controllers;

public class QuizController : Controller
{
    private readonly IQuizUserService _service;

    public QuizController(IQuizUserService userService)
    {
        _service = userService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Route( "/{id}" )]
    public ActionResult<QuizDto> FindById(int id)
    {
        var quiz = QuizDto.of( _service.FindQuizById( id ) );
        if (quiz == null)
        {
            return NotFound();
        }

        return Ok( quiz );
    }

    [HttpGet( "/Quizes" )]
    public IEnumerable<QuizDto> FindAll()
    {
        List<QuizDto> quiz = new List<QuizDto>();
        foreach (var item in _service.FindAllQuizzess())
        {
            quiz.Add( QuizDto.of( item ) );
        }
        return quiz;
    }

    [HttpPost]
    [Route( "{quizId}/items/{itemId}" )]
    public void SaveAnswer([FromBody] QuizItemAnswerDto dto, [FromRoute] int quizId,
        [FromRoute] int itemId)
    {
        _service.SaveUserAnswerForQuiz( quizId, dto.UserId, itemId, dto.Answer );
    }

    [HttpGet( "/{userId}/quizesAnswers/{quizId}" )]
    public QuizUserAnswerDto GetCorrectAnswers([FromRoute] int userId, [FromRoute] int quizId)
    {
        //List<QuizItemUserAnswer> quizes = _service.GetUserAnswersForQuiz( userId, quizId );
        //return _service.CountCorrectAnswersForQuizFilledByUser( quizId, userId );

        var answers = _service.GetUserAnswersForQuiz( quizId, userId )
            .Where( x => x.QuizId == quizId );

        return new QuizUserAnswerDto()
        {
            QuizId = quizId,
            UserId = 1,
            QuizAnswers = answers.Select( x => new QuizItemUserAnswersDto()
            {
                Question = x.QuizItem.Question,
                Answer = x.Answer,
                IsCorrect = x.IsCorrect(),
                QuizItemId = x.QuizItem.Id
            } ).ToList()
        };
    }

    [ApiController, Route( "/api/authentication" )]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<UserEntity> _manager;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger _logger;

        public AuthenticationController(UserManager<UserEntity> manager, ILogger<AuthenticationController> logger, IConfiguration configuration, JwtSettings jwtSettings)
        {
            _manager = manager;
            _logger = logger;
            _jwtSettings = jwtSettings;
        }

        [HttpPost( "login" )]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] LoginUserDto user)
        {
            if (!ModelState.IsValid)
            {
                return Unauthorized();
            }
            var logged = await _manager.FindByNameAsync( user.LoginName );
            if (await _manager.CheckPasswordAsync( logged, user.Password ))
            {
                return Ok( new { Token = CreateToken( logged ) } );
            }
            return Unauthorized();
        }

        private string CreateToken(UserEntity user)
        {
            return new JwtBuilder()
                .WithAlgorithm( new HMACSHA256Algorithm() )
                .WithSecret( Encoding.UTF8.GetBytes( _jwtSettings.Secret ) )
                .AddClaim( JwtRegisteredClaimNames.Name, user.UserName )
                .AddClaim( JwtRegisteredClaimNames.Gender, "male" )
                .AddClaim( JwtRegisteredClaimNames.Email, user.Email )
                .AddClaim( JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes( 5 ).ToUnixTimeSeconds() )
                .AddClaim( JwtRegisteredClaimNames.Jti, Guid.NewGuid() )
                .Audience( _jwtSettings.Audience )
                .Issuer( _jwtSettings.Issuer )
                .Encode();
        }
    }
}