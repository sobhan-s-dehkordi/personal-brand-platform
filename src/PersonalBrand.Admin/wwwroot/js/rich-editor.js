(() => {
    if (!window.Quill) return;
    document
        .querySelectorAll("textarea[data-rich-editor]")
        .forEach((source) => {
            const frame = document.createElement("div");
            frame.className = "rich-editor";
            const host = document.createElement("div");
            frame.append(host);
            source.after(frame);
            const editor = new Quill(host, {
                theme: "snow",
                formats: [
                    "bold",
                    "italic",
                    "underline",
                    "strike",
                    "link",
                    "list",
                    "blockquote",
                    "code",
                    "code-block",
                ],
                modules: {
                    toolbar: [
                        ["bold", "italic", "underline", "strike"],
                        [{ list: "ordered" }, { list: "bullet" }],
                        ["blockquote", "code-block", "link"],
                        ["clean"],
                    ],
                    history: { userOnly: true },
                },
            });
            editor.setContents(
                editor.clipboard.convert({
                    html: source.dataset.initialHtml || "",
                }),
                "silent",
            );
            editor.history.clear();
            editor.root.setAttribute("dir", source.getAttribute("dir") || "auto");
            editor.root.setAttribute("role", "textbox");
            editor.root.setAttribute("aria-multiline", "true");
            const label = document.querySelector(
                `label[for="${CSS.escape(source.id)}"]`,
            );
            editor.root.setAttribute(
                "aria-label",
                label?.textContent || "Content",
            );
            if (label) label.addEventListener("click", () => editor.focus());
            const tools = frame.querySelector(".ql-toolbar");
            tools.querySelectorAll("button").forEach((button) => {
                const format = [...button.classList]
                    .find((c) => c.startsWith("ql-"))
                    ?.slice(3);
                const name = `${format || "Format"}${button.value ? " " + button.value : ""}`;
                button.title = name;
                button.setAttribute("aria-label", name);
            });
            for (const [name, action] of [
                ["Undo", () => editor.history.undo()],
                ["Redo", () => editor.history.redo()],
            ]) {
                const button = document.createElement("button");
                button.type = "button";
                button.textContent = name;
                button.className = "editor-history";
                button.title = name;
                button.addEventListener("click", action);
                tools.append(button);
            }
            source.hidden = true;
            editor.on("text-change", () => {
                source.value = editor.getText().trim()
                    ? "pb-html:" +
                      editor.getSemanticHTML().replaceAll("&nbsp;", " ")
                    : "";
                source.dispatchEvent(new Event("change", { bubbles: true }));
            });
        });
})();
