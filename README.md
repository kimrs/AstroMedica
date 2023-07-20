# Beyond null: Embracing Safer Alternatives and Practices in Software Development
This repository houses a case study designed to illustrate the issues with using null in our code, and presents strategies for avoiding these problems.
The software under examination is a glucose analyzer created by the fictitious health tech company, Astro Medica.

This glucose analyzer consists of a backend server and a console application.
To run the server use the `Backend: https` run configuration.

You can run the LabAnswerAnalyser with the patient ID as a command-line argument, provided the server is up and running.
These are run configurations you can use for testing:
* `PhoneNumberPatient` a standard patient with a phone number but no email address.
* `CovidPatient` a covid patient who hasn't provided a phone number.
* `LegacyPatient` a patient who hasn't provided their zodiac sign.
* `LazyInitializedPatient` a patient added concurrently as the tool is run.

While it's encouraged to run the tool and experiment with the various patient profiles,
it's not strictly necessary to do so in order to follow the assignment.
The main goal of this exercise is to gain a solid understanding of null issues
and how to refactor your code to circumvent them effectively. Happy coding!

## Introduction
The null reference was invented by Sir Anthony Hoare in 1965 while working on ALGOL W,
a precursor to many modern programing languages such as Pascal, Simula and C.
His job was to develop a type-system that would ensure the safe usage of all references.
He solved this task by implementing objects and pointers that would point to the location of these objects.
And he included the object type with the pointer so we would have type checking for this.
These were all good ideas that have helped us develop complex systems in less time.
Unfortunately, he also invented the null reference.
So now, the reference can be either the type you specified or null.
And to avoid risking disaster, you must check for null every time you use it.

The null reference became a source of numerous software bugs, crashes and vulnerabilities.
And now, the "null pointer exception" is one of the most infamous software bugs.
Tony later apologized for this calling the invention his billion-dollar-mistake.
However, he does not seem willing to pay us back. So it is time for us programmers to take responsibility.
### The Many Faces of Null
```csharp
try
{
  result = await _httpClient.GetAsync($"patient/{id.Value}");
}

catch (HttpRequestException)
{
  return null;
}
```
When we return null, we are implicitly assigning meaning to it. It's often unclear whether a reference is null because:
* An unexpected error happened
* "null" is the default value of something
* A property remained uninitialized for some reason

This ambiguity makes it challenging for developers to reason about their code.
It increases the likelihood of bugs and hampers code maintainability.
In the example above, the implicit meaning we assign to null is
`Patient could not be retrieved due to connectivity issues.`
While this might be clear when we look at the method, this information is lost once we exit its scope.

Permitting the `return null` statement in your project can lead developers down the path of defensive programming,
adding null checks before accessing memory. Unfortunately, defensive programming often results in unnecessarily verbose code.
Worse still, in trying to circumvent potential issues, developers might overlook the unhappy path when adding null checks,
which can introduce elusive bugs. Consider the following code snippet I recently reviewed:
```csharp
var patient = await _patientService.Get(patientId);

if (patient is null)
{
  return;
}

var labAnswers = await _labAnswerService.ByPatientIdThrowIfNone(patient.Id);
...
```
When I encounter null checks such as this, I usually ask, "When is this object null?"
In this particular instance, the developer couldn't provide an answer, merely stating
that if the object did happen to be null, our system would continue to operate as intended.
The developer asserted that robust code is always better, implying that this kind of defensive
programming is synonymous with robustness.

However, I disagree. This is not an illustration of robust code. Rather, it's a clear neglect of
the unhappy path. If the `patient` object turns out to be null for some unforeseen reason,
the program could potentially become much harder to debug. This kind of null check might shield us
from immediate issues, but it leaves the door open for more complex, hidden problems down the line.

The fundamental misunderstanding that defensive programming equates to robust code, as my teammate
seemed to believe, is the motivation behind this article. Robust code shouldn't merely attempt to
circumvent problems; instead, it should anticipate them, handle them efficiently, and even more importantly,
endeavor to prevent them in the first place. Defensive programming is a part of this, but it is not
the whole story — the ability to reason about our code, to understand the situations when and why
an object might be null, is equally crucial.

# Astro Medicas' Glucose Analyzer
To illuminate the complexities of handling null references, let's delve into a hypothetical scenario
featuring a fictitious health tech company, Astro Medica. They've developed a glucose analyzer that,
for the most part, performs its function adequately. However, the developers are encountering inexplicable
errors cropping up in production. We're stepping into this scenario as the hired software consultants to
identify these mysterious issues and improve the system's maintainability. This scenario, though fictitious,
perfectly exemplifies the real-world challenges associated with handling null references, and how proper
understanding and treatment can lead to more robust and manageable software.

Astro Medica's glucose analyzer is designed to provide personalized health insights based on the patients' zodiac signs.
The device processes lab results, comparing the patient's glucose level against a preset threshold specific to their
zodiac sign. If the glucose reading is above this personalized threshold, the patient is promptly notified.

| Zodiac Sign | Glucose Tolerance |
|-------------|-------------------|
| Aries       | 30                |
| Taurus      | 40                |
| Gemini      | 45                |
| Cancer      | 41                |
| Leo         | 32                |
| Virgo       | 35                |
| Libra       | 36                |

The initial step the device performs is fetching the patient's data from the server. After the data is retrieved,
an immediate null check is performed. Should the patient data come up null, the system takes a pause, and then another
attempt is made to retrieve the data. The in-line comments shed some light on the two circumstances that might lead to
a null patient: the server could still be initializing or the patient data hasn't been added yet due to the system's
eventual consistency.

However, as we all know, code comments can be a double-edged sword. They're useful for understanding the developer's
intentions but can quickly become outdated or inaccurate as the code evolves. As such, we can't solely rely on these comments
for an accurate understanding of when or why null might occur. It behooves us to investigate further, to peek inside the
`_patientService` to validate these comments and better understand the conditions leading to a null patient.
```csharp
    public async Task HandleGlucoseAnalyzedForPatient(Id patientId)
    {
        var patient = await _patientService.Get(patientId);
        if (patient is null)
        {
            // There are two cases were we must try again
            //  1. The server is still initializing
            //  2. The patient has not yet been added because it is eventual consistent

            _logger.LogInformation("Trying again");
            await Task.Delay(TimeSpan.FromSeconds(10));
            await HandleGlucoseAnalyzedForPatient(patientId);
            return;
        }
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/416804cff0cb0908779eaef4c9070df2a67a6beb/LabAnswerAnalyser/GlucoseAnalyser.cs#L40-L51)

Upon examining the Get method in `PatientService`, we discover two instances where null is returned. These occur when the `_httpClient` throws an
`HttpRequestException`, or if the `JsonConvert.DeserializeObject` method throws a `JsonReaderException`. Here, we have a direct attribution of two
implicit meanings to null:

* The server is offline, represented by an `HttpRequestException`.
* Deserialization fails, leading to a `JsonReaderException`.

Interestingly, neither of these circumstances align with the two potential scenarios outlined in the code comments we examined earlier.
```csharp
public async Task<IPatient> Get(Id id)
{
    HttpResponseMessage result;
    try
    {
        result = await _httpClient.GetAsync($"patient/{id.Value}");
    }
    catch (HttpRequestException)
    {
        return null;
    }
    var jsonResponse = await result.Content.ReadAsStringAsync();

    try
    {
        return JsonConvert.DeserializeObject<IPatient>(jsonResponse, _settings);
    }
    catch (JsonReaderException)
    {
        return null;
    }
}
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/416804cff0cb0908779eaef4c9070df2a67a6beb/LabAnswerAnalyser/PatientService.cs#L23C1-L44)

Upon deeper investigation into the system, specifically the backend code, we start to unravel the original context of the comments. In the Read
controller method, we can see that null is returned in two situations - either the server is still in the initialization phase, or the patient
doesn't exist in the system.
```csharp
[HttpGet("{idValue}")]
public IPatient Read(int idValue)
{
    if (!InitializationTask.IsCompleted)
    {
        return null;
    }

    return PatientDb.TryGetValue(new Id(idValue), out var patient)
        ? patient
        : null;
}
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/416804cff0cb0908779eaef4c9070df2a67a6beb/Backend/PatientController.cs#L36-L49)

With this discovery, we have now assigned four different implicit meanings to null throughout the system:
* The server is offline
* Deserialization fails
* Server is still initializing
* The patient does not exsist

Four meanings for a single construct is a significant overloading of responsibility, especially for something as
fundamental as null. Given that null has already been described as a "billion-dollar mistake" by its creator, assigning
so many interpretations complicates matters even more. However, as we will see in the upcoming chapters, there are ways to mitigate these issues.

# The `IOption<T>` Interface
Before we delve further into possible solutions for our 'null' conundrum, let's first consider exceptions - a crucial tool
in error handling within any software system.

Exceptions should be used to handle scenarios that fall outside the normal operation of our business domain - particularly
when an unexpected condition is encountered, causing the regular flow of the program to halt. If such an exception occurs,
it's often more desirable for the system to fail immediately. Doing so provides several advantages:

* Faster Error Discovery: By causing an immediate halt, exceptions help reveal the occurrence of an error promptly, leading to faster detection and resolution.
* Easier Debugging: As exceptions provide a snapshot of the program's state at the moment of failure, they can serve as useful pointers to the error's source,
thus making the debugging process easier.
* Preventing Unexpected Behaviour: By failing fast, we prevent the program from continuing in an erroneous state, which could lead to unpredictable behavior
and potentially more severe problems down the line.
Remember, an error isn't always a bad thing if it halts execution. Rather than silently ignoring or masking the problem, causing immediate failure can be a
more responsible way to handle unexpected scenarios. By recognizing and responding to errors appropriately, we can maintain a healthier, more robust codebase.

In the case of our glucose analyzer from Astro Medica, the absence of a patient in the system might seem like a business domain problem at first glance.
But, if we consider that the patient is supposed to be present when the glucose analysis takes place, the patient's absence becomes a technical error, not
a business one.

This design pattern, often known as the "Option pattern" or "Maybe pattern," is a powerful tool in helping to manage situations where a value may or may not
be present due to technical issues. Here, instead of returning a null reference and losing information about why a value is missing, you wrap the value within
an IOption<T> interface. The IOption can either be a Some (holding the value) or a None (indicating the absence of a value, along with a reason).

```csharp
public interface IOption<out T>
{
    T EnsureHasValue();
}

public record Some<T>(T Value) : IOption<T>
{
    public T EnsureHasValue()
    {
        if (Value is null)
        {
            throw new NullReferenceException(nameof(Value));
        }

        return Value;
    }
}

public record None<T>(IReason Because) : IOption<T>
{
    public T EnsureHasValue()
    {
        throw Because.Exception;
    }
}

public interface IReason
{
    Exception Exception { get; }
}

public record ServiceUnavailable
    : IReason
{
    public Exception Exception => new("Service unavailable, message placed on the error queue");
}

public record ServiceNotYetInitialized
    : IReason
{
    public Exception Exception => new("Service is still initializing. Retrying in a couple of minutes");
}
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/416804cff0cb0908779eaef4c9070df2a67a6beb/Transport/Option.cs#L3-L44)

In this code, the `EnsureHasValue()` method will either return the value (if it exists) or throw an exception detailing the reason
for the absence of the value. This significantly improves clarity in debugging, as it captures the reason for the error directly in
the code execution path. This pattern helps create a more maintainable and less error-prone codebase by making the absence of a value
explicit and informative.

## Implementing the `IOption<T>` in the glucose analyzer scenario
We start by using `IOption<T>` in `PatientService`. Instead of returning a `Patient`, we return `IOption<IPatient>`. This eliminates the need
to return null when an exception occurs; instead, we return `None` along with the appropriate reason.

```csharp
    public async Task<IOption<IPatient>> Get(Id id)
    {
        HttpResponseMessage result;
        try
        {
            result = await _httpClient.GetAsync($"patient/{id.Value}");
        }
        catch (HttpRequestException)
        {
            return new None<IPatient>(new ServiceUnavailable());
        }
        var jsonResponse = await result.Content.ReadAsStringAsync();

        try
        {
            return JsonConvert.DeserializeObject<IOption<IPatient>>(jsonResponse, _settings);
        }
        catch (JsonReaderException)
        {
            return new None<IPatient>(new FailedToDeserialize());
        }
    }
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/1494e6fe8412e63fb7c46388dd5cfdea5689df5a/LabAnswerAnalyser/PatientService.cs#L24-L45C6)

Usually, your controller method should return an `ActionResult`, wrapping the returned content. For instance, it would be more appropriate
to return a 404 status if an item doesn't exist. However, to illustrate our point, let's implement `IOption` in our endpoint:

```csharp
    [HttpGet("{idValue}")]
    public IOption<IPatient> Read(int idValue)
    {
        if (!InitializationTask.IsCompleted)
        {
            return new None<IPatient>(new ServiceNotYetInitialized());
        }

        return PatientDb.TryGetValue(new Id(idValue), out var patient)
            ? new Some<IPatient>(patient)
            : new None<IPatient>(new ItemDoesNotExist());
    }
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/1494e6fe8412e63fb7c46388dd5cfdea5689df5a/Backend/PatientController.cs#L38-L49C4)

With the `_patientService` now returning an `IOption` instead of null, we can improve the `GlucoseAnalyzer`. Our code becomes more descriptive,
eliminating the need for comments explaining potential null returns.

```csharp
var maybePatient = await _patientService.Get(patientId);
if (maybePatient is None<IPatient> {Because: ItemDoesNotExist or ServiceNotYetInitialized})
{
    _logger.LogInformation("Trying again");
    
    await Task.Delay(TimeSpan.FromSeconds(10));
    await HandleGlucoseAnalyzedForPatient(patientId);
    return;
}

var patient = maybePatient.EnsureHasValue();
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/1494e6fe8412e63fb7c46388dd5cfdea5689df5a/LabAnswerAnalyser/GlucoseAnalyser.cs#L41-L51)

Please note, the condition in the if clause only covers potential null returns as indicated by the old comments. It excludes cases we uncovered
where null was returned due to an unreachable server or failed deserialization. I argue that these were bugs in the original system. If the server
is off, it's unlikely to be back up within the next 10 seconds. Similarly, if deserialization failed once, it's not likely that waiting will resolve the issue.


# Eliminating Nullable Properties through Better Modeling
Often, the presence of 'null' values in code can indicate a problem with the modeling. In particular, 
using enum values for type information can lead to issues, a phenomenon I've observed quite often.
We'll explore this in more detail in the following section.

After retrieving the patient details, we proceed to fetch the lab answers. The name of the method,
`ByPatientThrowIfNone`, suggests that it throws an exception if it's unable to retrieve any lab answers
for a given patient. This use of exceptions indicates that it's not part of the business logic, i.e.,
the glucose analyzer is designed to only run if lab answers exist. After retrieving the lab answer, we
perform a null check on `GlucoseLevel`. There's no explicit explanation for when `GlucoseLevel` might be null,
but I suspect it has something to do with the `ExaminationType` enum.

```csharp
    var labAnswers = await _labAnswerService.ByPatientThrowIfNone(patient.Id);
    var labAnswer = labAnswers.Where(x => x.ExaminationType == ExaminationType.Glucose).First();
    if (labAnswer.GlucoseLevel == null)
    {
        throw new NullReferenceException(nameof(labAnswer.GlucoseLevel));
    }
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/6b34199e3ee06fe768a107d5744ae7c8aa13c056/LabAnswerAnalyser/GlucoseAnalyser.cs#L53-L58)

Upon inspecting the `LabAnswer` class, it's apparent that it includes both a `GlucoseLevel` and a nullable `BinaryLabAnswer`, along with an `ExaminationType`.

```csharp
public record LabAnswer(GlucoseLevel GlucoseLevel, BinaryLabAnswer? BinaryLabAnswer, ExaminationType ExaminationType)
{
    public static bool operator >(LabAnswer a, GlucoseLevel b) => a.GlucoseLevel > b;
    public static bool operator <(LabAnswer a, GlucoseLevel b) => a.GlucoseLevel < b;

    public override string ToString() => ExaminationType switch
    {
        ExaminationType.Glucose => $"{nameof(LabAnswer)}:{GlucoseLevel}",
        ExaminationType.Covid19 => $"{nameof(LabAnswer)}:{BinaryLabAnswer}",
        _ => string.Empty
    };
}
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/6b34199e3ee06fe768a107d5744ae7c8aa13c056/Transport/Lab/LabAnswer.cs#L3C1-L14)

Further examination of the database (a `Dictionary` in this case) reveals that `LabAnswer` instances with
`ExaminationType` = Glucose have a `GlucoseLevel` but not a `BinaryLabAnswer`. Conversely, `LabAnswer` instances
with `ExaminationType` = Covid19 have a `BinaryLabAnswer` but not a `GlucoseLevel`. This implies that a null value
in these cases indicates that the property is not relevant for the specific examination type.

```csharp
    private static readonly Dictionary<Id, List<LabAnswer>> LabAnswerDb = new()
    {
        {
            new Id(0),
            new List<LabAnswer>
            {
                new (
                    new GlucoseLevel(60),
                    BinaryLabAnswer: null,
                    ExaminationType.Glucose
                )
            }
        },
        {
            new Id(1),
            new List<LabAnswer>
            {
                new (
                    new GlucoseLevel(50),
                    BinaryLabAnswer: null,
                    ExaminationType.Glucose
                ),
                new (
                    GlucoseLevel: null,
                    BinaryLabAnswer.Positive,
                    ExaminationType.Covid19
                )
            }
        },
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/6b34199e3ee06fe768a107d5744ae7c8aa13c056/Backend/LabAnswerController.cs#L12-L40)

At this point, it may become clear that the LabAnswer class is overreaching in its responsibilities,
leading to null values in places where certain properties are not applicable. To remedy this, `LabAnswer`
can be split into two distinct classes: `GlucoseLabAnswer` and `Covid19LabAnswer`. This change clarifies the
responsibilities of each class and eliminates unnecessary null values.

```csharp
public interface ILabAnswer { }

public record GlucoseLabAnswer(GlucoseLevel GlucoseLevel)
    : ILabAnswer
{
    public static bool operator >(GlucoseLabAnswer a, GlucoseLevel b) => a.GlucoseLevel > b;
    public static bool operator <(GlucoseLabAnswer a, GlucoseLevel b) => a.GlucoseLevel < b;
    public override string ToString() => $"{nameof(GlucoseLabAnswer)}:{GlucoseLevel}";
}

public record Covid19LabAnswer(BinaryLabAnswer BinaryLabAnswer)
    : ILabAnswer
{
    public override string ToString() => $"{nameof(Covid19LabAnswer)}:{BinaryLabAnswer}";
}
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/661aa6eee88d1789eed330dfb649eaac2e4c3f26/Transport/Lab/LabAnswer.cs#L3-L17)

Of course, this change necessitates the migration of our database.

```csharp
public class LabAnswerController
{
    private static readonly Dictionary<Id, List<ILabAnswer>> LabAnswerDb = new()
    {
        {
            new Id(0),
            new List<ILabAnswer>
            {
                new GlucoseLabAnswer(new GlucoseLevel(60))
            }
        },
        {
            new Id(1),
            new List<ILabAnswer>
            {
                new GlucoseLabAnswer(new GlucoseLevel(50)),
                new Covid19LabAnswer(BinaryLabAnswer.Positive)
            }
        },
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/661aa6eee88d1789eed330dfb649eaac2e4c3f26/Backend/LabAnswerController.cs#L10C10-L28)

Now that we've removed the enum that undermined the type system, we can and should use the `OfType` method, rather than `Where`.
Also, as we are no longer setting the `GlucoseLevel` to null, the null-check is no longer necessary.

```csharp
    var labAnswers = await _labAnswerService.ByPatientThrowIfNone(patient.Id);
    var labAnswer = labAnswers.OfType<GlucoseLabAnswer>().First();
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/661aa6eee88d1789eed330dfb649eaac2e4c3f26/LabAnswerAnalyser/GlucoseAnalyser.cs#L53-L54)

The `OfType` method is more appropriate here as it filters the collection based on the type, which is more idiomatic and efficient in this case compared to `Where`.

# Leveraging Interfaces for Variability
Sometimes, in software design, we encounter scenarios where different classes represent variations of the same conceptual entity.
A slight difference in characteristics can lead to the presence of null values in some properties, indicating they are not relevant for
certain variants. Failing to express this difference in our code often leads to these null values. We will explore an approach to address
this issue in this section using interfaces.

After retrieving the LabAnswer, we decide whether to notify the patient based on their zodiac sign. However, we have some patients registered
before we incorporated astrology into our system. For these "legacy" patients, we don't have zodiac signs, and we resort to using a default
glucose tolerance. Implicitly, by using null for the `ZodiacSign`, we say that null means the property was not relevant at the time of patient
registration. So let us remove this implicit meaning.

```csharp
    // We do not have the zodiac sign of patients that registered before we incorporated astrology
    var shouldNotifyPatient = patient.ZodiacSign is not null
        ? labAnswer.GlucoseLevel > _glucoseTolerance.ZodiacTolerance[patient.ZodiacSign.Value]
        : labAnswer.GlucoseLevel > _glucoseTolerance.DefaultTolerance;
        
    if (!shouldNotifyPatient)
    {
        return;
    }
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/6b34199e3ee06fe768a107d5744ae7c8aa13c056/LabAnswerAnalyser/GlucoseAnalyser.cs#L60-L68C10)

Thus, we need to accommodate two types of patients in our code - ordinary patients who have a zodiac sign and legacy patients who don't.
This dichotomy, however, is not currently expressed in our code.

```csharp
public record Patient(
    Id Id,
    Name Name,
    ZodiacSign? ZodiacSign,
    IPhoneNumber PhoneNumber,
    IMailAddress MailAddress
) : IPatient;
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/6b34199e3ee06fe768a107d5744ae7c8aa13c056/Transport/Patient/Patient.cs#L12-L18)

As before, splitting the class into `Patient` and `LegacyPatient` could resolve this issue. However, to maintain flexibility for future expansion
(in case we need to accommodate more types of patients), we could encapsulate the presence of a `ZodiacSign` in an interface - `IHasZodiacSign`.

```csharp
public interface IHasZodiacSign
{
    ZodiacSign ZodiacSign { get; }
}

public record Patient(
    Id Id,
    Name Name,
    ZodiacSign ZodiacSign,
    IPhoneNumber PhoneNumber,
    IMailAddress MailAddress
) : IPatient, IHasZodiacSign;

public record LegacyPatient(
    Id Id,
    Name Name,
    IPhoneNumber PhoneNumber,
    IMailAddress MailAddress
) : IPatient;
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/8741996d2150b903f07ebaeddfdca02a16a0290a/Transport/Patient/Patient.cs#L11-L29)

The glucose analyzer can then leverage this interface, making the decision process to notify the patient more explicit and intuitive:

```csharp
    var shouldNotifyPatient = patient is IHasZodiacSign hasZodiacSign
        ? labAnswer.GlucoseLevel > _glucoseTolerance.ZodiacTolerance[hasZodiacSign.ZodiacSign]
        : labAnswer.GlucoseLevel > _glucoseTolerance.DefaultTolerance;
        
    if (!shouldNotifyPatient)
    {
        return;
    }
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/8741996d2150b903f07ebaeddfdca02a16a0290a/LabAnswerAnalyser/GlucoseAnalyser.cs#L56-L63)

This solution eliminates the need for the comment explaining the null `ZodiacSign`, as the code now expresses this concept more clearly
through the use of an interface. Additionally, this design adheres to the Open/Closed principle, allowing the system to easily accommodate
new types of patients in the future without needing to modify existing code.

To summarize, by leveraging interfaces, we were able to express variability in our `Patient` class, improving the clarity of our code and
eliminating unnecessary null checks.

# Representing Optional Data Explicitly
The use of null to signify optional or unprovided data by a user is common. However, this approach can lead to ambiguity and unnecessary
null checks. The solution to this problem can be quite straightforward and simply involves representing this scenario with a distinct class.

When examining the process of notifying the patient, we find that a patient is either notified through text or mail, based on whether they
have provided us with a phone number. This decision is derived through a null-check, hence, implicitly stating that null signifies an unconfigured
and optional property.

```csharp
    if (patient.PhoneNumber is not null)
    {
        _smsService.TellPatientToEatLessSugar(patient.PhoneNumber, labAnswer);
        return;
    }
        
    if (patient.MailAddress is not null)
    {
        _mailService.TellPatientToEatLessSugar(patient.MailAddress, labAnswer);
    }
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/6b34199e3ee06fe768a107d5744ae7c8aa13c056/LabAnswerAnalyser/GlucoseAnalyser.cs#L70C32-L79)

However, the reliance on null for this decision could lead to potential pitfalls. For instance, if due to an unexpected event, the
PhoneNumber or MailAddress properties were set to null, it could lead to the patient not receiving a notification despite having provided
us with a phone number. The clarity provided by explicit representation helps us distinguish between the different scenarios where a patient
did not provide us with their contact information and when an unexpected event causes a property to be null.

Here is how we can enhance this code by being more explicit.

```csharp
public interface IPhoneNumber { }

public record PhoneNumberNotSet : IPhoneNumber;

public record PhoneNumber(string Value)
    : IPhoneNumber
{
    public override string ToString() => Value;
}

public interface IMailAddress { }

public record MailAddressNotSet : IMailAddress;

public record MailAddress(string Value)
    : IMailAddress
{
    public override string ToString() => Value;
}
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/5c7b956dfbfae750db7c7468f76f27b240a67cea/Transport/Patient/Patient.cs#L31-L49)

By introducing classes such as PhoneNumberNotSet and MailAddressNotSet, we can effectively convey that the patient did not provide this information and that this is a common business case.

In line with this change, we also need to migrate our patient database:

```csharp
    private static readonly Dictionary<Id, IPatient> PatientDb = new List<IPatient>()
    {
        new Patient(
            new Id(0),
            new Name("Tony Hoare"),
            ZodiacSign.Aries,
            new PhoneNumber("815 493 00"),
            MailAddress: new MailAddressNotSet()
        ),
        new Patient(
            new Id(1),
            new Name("Ada Lovelace"),
            ZodiacSign.Gemini,
            PhoneNumber: new PhoneNumberNotSet(),
            MailAddress: new MailAddressNotSet()
        ),
        new LegacyPatient(
            new Id(2),
            new Name("Brian Kernighan"),
            PhoneNumber: new PhoneNumberNotSet(),
            new MailAddress("Portveien 2")
        )
    }.ToDictionary(x => x.Id);
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/5c7b956dfbfae750db7c7468f76f27b240a67cea/Backend/PatientController.cs#L11C12-L33)

Finally, in our glucose analyzer, the check now looks as follows:

```csharp
    if (patient.PhoneNumber is not PhoneNumberNotSet)
    {
        _smsService.TellPatientToEatLessSugar(patient.PhoneNumber, labAnswer);
        return;
    }
        
    if (patient.MailAddress is not MailAddressNotSet)
    {
        _mailService.TellPatientToEatLessSugar(patient.MailAddress, labAnswer);
    }
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/5c7b956dfbfae750db7c7468f76f27b240a67cea/LabAnswerAnalyser/GlucoseAnalyser.cs#L65C1-L74C10)

This revised approach makes the code more explicit, more predictable, and less prone to bugs. It's also a
useful pattern to consider in any scenario where null is being used to represent the absence of data.

# Enabling Null-State Analysis and Addressing Warnings
In this last section, we will activate the Null-state analysis feature that comes with the nullable reference type configuration.
This static analysis tool keeps track of the null state of reference types and issues warnings when a null dereference could occur.
To enable this, we modify the project file by adding the `Nullable` element:

```xml
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net7.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/449ce279fa943408153c73a7c40719ec8396083b/LabAnswerAnalyser/LabAnswerAnalyser.csproj#L3-L8)

With this feature enabled, we start to see new warnings. One such warning appears in the `PatientService` when the service attempts
to deserialize the server's response. The issue arises because the `DeserializeObject` method can return null if it's provided with null.
By handling the case where it returns null, we can eliminate this warning:

```csharp
    try
    {
        return JsonConvert.DeserializeObject<IOption<IPatient>>(jsonResponse, _settings)
               ?? new None<IPatient>(new FailedToDeserialize());
    }
    catch (JsonReaderException)
    {
        return new None<IPatient>(new FailedToDeserialize());
    }
```
[snippet source](https://github.com/kimrs/AstroMedica/blob/449ce279fa943408153c73a7c40719ec8396083b/LabAnswerAnalyser/PatientService.cs#L37-L41C8)

With this update, we handle the potential null return of `DeserializeObject` method by returning a `None<IPatient>` with a `FailedToDeserialize`
reason, thereby explicitly representing the lack of a patient result due to a failed deserialization.

# Summary
We began with an application that assigned seven distinct interpretations to the null value, each corresponding to a different scenario:

* The server is offline
* The server is initializing
* Failure to deserialize the server's response
* Item was not found
* Property is not applicable for a given type
* Property was not relevant at the time of object creation
* Property is optional and has not been configured

Our challenge was to refactor the code to reduce ambiguity and improve clarity.
By using five techniques, we successfully eliminated all implicit meanings assigned to null.

* IOption<T>
* Better modeling
* Leveraging interfaces
* Explicitly representing optional data
* Enabling nullable reference types

By applying these methods, we have transformed the application into a more maintainable, bug-resilient structure, thereby enhancing its long-term sustainability.
