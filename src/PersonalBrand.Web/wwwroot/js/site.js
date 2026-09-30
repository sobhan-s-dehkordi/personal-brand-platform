document.querySelectorAll("pre").forEach((pre) => {
    const code = pre.querySelector("code");
    const source = code?.textContent || pre.textContent;
    if (
        code &&
        (code.className.includes("csharp") || code.className.includes("cs"))
    ) {
        const expression =
            /(\/\/[^\n]*|"(?:[^"\\]|\\.)*"|\b(?:public|private|class|sealed|return|if|throw|new|bool|int|string|async|await|var|using)\b|\b\d+\b)/g;
        let last = 0;
        const fragment = document.createDocumentFragment();
        for (const match of source.matchAll(expression)) {
            fragment.append(
                document.createTextNode(source.slice(last, match.index)),
            );
            const span = document.createElement("span");
            span.className = match[0].startsWith("//")
                ? "code-comment"
                : match[0].startsWith('"')
                  ? "code-string"
                  : "code-keyword";
            span.textContent = match[0];
            fragment.append(span);
            last = match.index + match[0].length;
        }
        fragment.append(document.createTextNode(source.slice(last)));
        code.replaceChildren(fragment);
    }
    const button = document.createElement("button");
    button.textContent =
        document.documentElement.lang === "fa" ? "کپی کد" : "Copy code";
    button.type = "button";
    button.className = "btn btn-light";
    button.addEventListener("click", async () => {
        try {
            await navigator.clipboard.writeText(source);
            button.textContent = "✓";
        } catch {
            button.textContent =
                document.documentElement.lang === "fa"
                    ? "متن را انتخاب و کپی کنید"
                    : "Select and copy";
        }
    });
    pre.after(button);
});
