# aspnet-identity-backend-starter

A starting point for an ASP.NET Core backend using **ASP.NET Core Identity**.

## Purpose
I created this project as a way to re-enforce some knowledge about ASP.NET backends as well as learn new things and ensure I knew the fundamentals well enough to create a project from scratch. Although I had worked in a ASP.NET backend before it was created by others and I followed the existing patterns within it. This small project was invaluable to looking into the different parts at a deeper level and ensuring I had a good understanding of the concepts. It should also serve as a good reference and starting point for me in similar projects down the road.

## Compromises
There is no front-end associate with this and as a result there is some clunkiness (non-best practices) around password reset flow. The implementation sends you the token as part of your email body so that you can then copy-paste it into whatever you are using to hit the endpoints. Obviously not ideal and if you use this as a starting point for your project you should modify those endpoints to send a link to an actual front-end with a form, which will in turn hit the correct endpoint with the token and new password.

Features:
- JWT authentication
- Email verification
- Password reset flows
- Clear separation of **Auth** and **Application** domain logic via a `UserProfile` model

---

## Database

This project uses a local sqlite database. To initalize the database locally open up a command prompt in the project root and execute 
`dotnet ef database update`

You may need to install the CLI tools with

`dotnet tool install --global dotnet-ef`

## Environment Variables

Sensitive configuration values (such as the SendGrid API key) are provided via **environment variables**.

### Local development

Create a `.env` file in the project root (this file should **not** be committed to git):

```env
JwtSettings__Secret=SuperAwesomeSecretKeyGoesHere
EmailSenderOptions__SendGridApiKey=SG.KeyYouGetFromSendGrid
EmailSenderOptions__FromEmail=myEmail@test.com
EmailSenderOptions__FromName="Awesome Email Sender Name"
