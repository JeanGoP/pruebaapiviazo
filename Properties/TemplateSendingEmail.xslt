<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  exclude-result-prefixes="msxsl">

	<xsl:output method="html" omit-xml-declaration="yes" indent="yes"/>
	<xsl:variable name="urlReserva" select="/top/DataInfo/@url"/>
	<xsl:template match="/">
		<html lang="es">
			<head>
				<meta charset="UTF-8"/>
				<meta name="viewport" content="width=device-width, initial-scale=1.0"/>
				<title>Confirmación de Reserva</title>
				<style>
					body {
					font-family: 'Montserrat', Arial, sans-serif;
					background-color: #e5e7eb;
					color: #082338 !important; /* azul oscuro elegante para textos */
					margin: 0;
					padding: 30px;
					text-decoration: none;
					}
					.container {
					background-color: #ffffff;
					border-radius: 12px;
					max-width: 600px;
					margin: auto;
					box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
					overflow: hidden;
					border: 1px solid #ddd;
					}
					.header {
					background-color: 082338;
					background-image: url('https://img.freepik.com/foto-gratis/big-ben-puente-westminster-al-atardecer-londres-reino-unido_268835-1395.jpg?semt=ais_hybrid&amp;w=740');
					background-size: cover;
					background-position: center;
					padding: 40px 20px;
					color: white;
					text-align: center;
					-webkit-text-stroke: 1px #082338;
					}
					.header h1 {
					font-size: 32px;
					margin: 0;
					text-shadow: 2px 2px 4px rgba(0,0,0,0.5);
					}
					.header h2 {
					font-size: 18px;
					margin: 10px 0 0 0;
					}
					.card-body {
					padding: 30px;
					}
					.details p {
					font-size: 16px;
					line-height: 1.6;
					margin: 10px 0;
					color: #082338; /* asegura que este color se respete */
					}
					.details strong {
					color: #082338;
					}
					.button {
					display: inline-block;
					background-color: #082338;
					color: white !important;
					padding: 12px 20px;
					border-radius: 6px;
					text-decoration: none;
					font-weight: bold;
					margin-top: 20px;
					}
					.footer {
					font-size: 12px;
					color: #666; /* más suave que el texto principal */
					text-align: center;
					padding: 15px;
					border-top: 1px solid #ddd;
					}
				</style>

			</head>
			<body>
				<div class="container">
					<div class="header">
						<h1>FootWay Tours</h1>
						<h1>¡Reserva Confirmada!</h1>
						<h2>Gracias por elegirnos. ¡Disfruta tu experiencia!</h2>
					</div>
					<div class="card-body">
						<div class="details">
							<p>
								Estimado(a) <strong>
									<xsl:value-of select="/top/DataInfo/@Nombre"/>
								</strong>,
							</p>
							<p>Tu reserva ha sido confirmada con los siguientes detalles:</p>
							<p>
								<strong>TOUR: </strong>
								<xsl:value-of select="/top/DataInfo/@NombreTour"/>
							</p>
							<p>
								<strong>FECHA: </strong>
								<xsl:value-of select="/top/DataInfo/@fecha"/>
							</p>
							<p>
								<strong>HORA: </strong>
								<xsl:value-of select="/top/DataInfo/@hora"/>
							</p>

							<a href="{$urlReserva}" class="button">Ver mi reserva</a>

						</div>
					</div>
					<div class="footer">
						<p>Este es un correo informativo, por favor no responda a este mensaje.</p>
					</div>
				</div>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>
