CalculateX
==========

CalculateX - Calculator that evaluates math expressions

![CalculateX](https://raw.githubusercontent.com/skst/CalculateX/master/CalculateX.png)

## CalculateX Features

### Summary
* Evaluate math expressions including grouping
* Support trigonometry and other functions
* Common unit conversion of the following types
    * Length
    * Speed
    * Temperature
    * Time
    * Mass
    * Volume
* Variable support including last answer

### Usage

Type an expression in the input field and press **Enter**.

Press the **up- and down-arrows** to cycle through the input history.

You can use `pi` and `e` as variables that represent those values.

You can use `answer` as a variable to represent the most recent result.
If you type an operator into an empty input field,
it will automatically type "`answer`" in the field for you.

### Basic Mathematical Operators

CalculateX supports all of the basic mathematical operators: addition, subtraction,
multiplication, and division. It also supports modulus and exponents.
You can use parentheses to group expressions and give them precedence.

* addition: `+`
* subtraction: `-`
* multiplication: `*` or `x` or implied (as in algebra)
* division: `/`
* modulus: `%`
* exponent: `^`

Example: `(3 + 4)(5 - 2) * (sin(5 x 9) / sqrt(25)) + max(16, 7) ^ 2`

### Basic Mathematical Functions

* sqrt(n) - square root of `n`
* abs(n) - absolute value of `n`
* pow(n, x) - `n` to the power of `x`
* min(a, b) - smallest of `a` and `b`
* max(a, b) - largest of `a` and `b`
* round(d, n) - `d` rounded to `n` decimal places
* floor(n) - largest integer smaller than `n`
* ceiling(n) - smallest integer larger than `n`

### Trigonometric Functions

* cos(t) - cosine of `t`
* cosh(t) - hyperbolic cosine of `t`
* acos(t) - arccosine of `t`
* sin(t) - sine of `t`
* sinh(t) - hyperbolic sine of `t`
* asin(t) - arcsine of `t`
* tan(t) - tangent of `t`
* tanh(t) - hyperbolic tangent of `t`
* atan(t) - arctangent of `t`

### Logarithmic Functions

* exp(x) - `e` to the power of `x`
* log(n) - log base `e` of `n`
* log10(n) - log base 10 of `n`

### Conversion Functions

To convert between two units, use this format:

`n[from->to]`

Example: `80[kph->mph]`

#### length

* Kilometer `mm`
* Meter `m`
* Centimeter `cm`
* Millimeter `mm`
* Mile `mile`
* Yard `yd`
* Feet `ft`
* Inch `in`

#### speed

* Kilometers/hour `kph`
* Miles/hour `mph`

#### temperature

* Fahrenheit `f`
* Celsius `c`
* Kelvin `k`

#### time

* Week `wk`
* Day `d`
* Hour `hr`
* Minute `min`
* Second `sec`
* Millisecond `ms`

#### mass

* Kilogram `kg`
* Gram `g`
* Milligram `mg`
* Ton `ton`
* Pound `lg`
* Ounce `oz`

#### volume

* Kiloliter `kl`
* Liter `l`
* Milliliter `ml`
* Gallon `gal`
* Quart `qt`
* Pint `pt`
* Cup `cup`
* Ounce `oz`

## MathExpressions Library

The library supports math expressions, functions, unit conversion, and variables.

Below are some C# examples of using the library directly.

````
MathEvaluator eval = new MathEvaluator();
//basic math
double result = eval.Evaluate("(2 + 1) * (1 + 2)");
//calling a function
result = eval.Evaluate("sqrt(4)");
//evaluate trigonometric 
result = eval.Evaluate("cos(pi * 45 / 180.0)");
//convert inches to feet
result = eval.Evaluate("12 [in->ft]");
//use variable
result = eval.Evaluate("answer * 10");
//add variable
eval.Variables.Add("x", 10);
result = eval.Evaluate("x * 10");
````

------------

*This project was forked from [Calculator.NET](https://github.com/loresoft/Calculator).*
