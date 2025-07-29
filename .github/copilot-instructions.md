Maintain existing code structure and organization.
Write unit tests for new functionality. Use table-driven unit tests when possible.
Document complex logic. Suggest changes to the `README.md` when appropriate.

Do not delete existing comments.
Prefer to use explicit type instead of var. Example: string s = new();
Prefer to assign new() or [] where possible.
Use tabs instead of spaces, even in XAML. A tab is equivalent to three spaces.
Use braces around single-line expressions.
Use parentheses around binary conditional expressions. Example: `if ((x > 0) && (y > 0))` but `if (x && y)`
Add a comma after the last item in an initializer list.

Do not add build artifacts to git. Ignore directories such as bin/ and obj/.
