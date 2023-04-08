<html>
    <head>
        <title>${headline}</title>
        <link rel="stylesheet" href="/assets/Default.css">
    </head>
    <body>
        <c:if spec="${sessionuser == null}">
            <a href="/sigin">Signin</a>
        </c:if>
        <c:content/>
    </body>
</html>