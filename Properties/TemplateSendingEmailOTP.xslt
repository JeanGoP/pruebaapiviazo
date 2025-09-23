<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output method="html" encoding="UTF-8" indent="yes"/>

	<xsl:template match="/">
		<html>
			<head>
				<style>
					body {
					font-family: Arial, sans-serif;
					background-color: #f4f4f4;
					padding: 20px;
					}
					.container {
					background-color: #ffffff;
					border-radius: 8px;
					padding: 30px;
					box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
					max-width: 500px;
					margin: auto;
					}
					.otp {
					font-size: 24px;
					font-weight: bold;
					color: #007BFF;
					margin: 20px 0;
					}
				</style>
			</head>
			<body>
				<div class="container">
					<h2>Verificación de Código OTP</h2>
					<p>Gracias por usar nuestros servicios. Tu código de verificación es:</p>
					<div class="otp">
						<xsl:value-of select="/top/DataInfo/@otp"/>
					</div>
					<p>Este código expirará en unos minutos. Si no lo solicitaste, ignora este mensaje.</p>
					<p>
						Atentamente,<br/>FootWayTours | London
					</p>
				</div>
			</body>
		</html>
	</xsl:template>

</xsl:stylesheet>
