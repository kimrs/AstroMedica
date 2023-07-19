# Beyond null: Embracing Safer Alternatives and Practices in Software Development
This repository houses a case study designed to illustrate the issues with using null in our code, and presents strategies for avoiding these problems.
The software under examination is a glucose analyzer created by the fictitious health tech company, Astro Medica.

This glucose analyzer consists of a backend server and a console application.
You can run the LabAnswerAnalyser with the patient ID as a command-line argument, provided the server is up and running.
Here are some IDs you can use for testing:
* 0 - Tony Hoare, a standard patient with a phone number but no email address.
* 1 - Ada Lovelace, a patient who hasn't provided a phone number.
* 2 - Brian Kernighan, a patient who hasn't provided their zodiac sign.
* 3 - Grace Hopper, a patient added concurrently as the tool is run.

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
And to avoid risking disaster, you must check for null every time you use it~~~~.

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
