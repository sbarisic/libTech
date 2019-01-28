// ==============
// LUN3DM5.SHADER
//
// You're free to use all of this, as always, for non-commercial stuff.
// If you release another Q3 map with these textures/shaders, and you add
// or modify anything, move everything out of lun3dm5 to avoid conflicts.
//
// ==============


textures/lun3dm5/lblusky2
{
	qer_editorimage env/lbsky2_lf.tga
	surfaceparm nolightmap
	surfaceparm noimpact
	surfaceparm nomarks
	surfaceparm sky
	skyparms env/lbsky2 720 -

	// 122 suns :)
	//				R		G		B		bright	yaw		pitch	dev	samp
	q3map_sunExt	1.00	0.90	0.80	180.0	17.123	48.822	0	16		// the big yellow one

	q3map_sunExt	0.7329	0.9208	1.0		10.6513	89.613	-66.69	2	16		// underlights
	q3map_sunExt	0.5994	0.8315	1.0		10.8072	258.36	-71.28	2	16
	q3map_sunExt	0.5990	0.8390	1.0		10.4978	327.14	-72.79	2	16
	q3map_sunExt	0.8041	0.9418	1.0		11.0879	33.591	-70.25	2	16
	q3map_sunExt	0.7405	0.9088	1.0		10.7015	197.01	-68.93	2	16
	q3map_sunExt	0.7783	0.9246	1.0		10.9546	142.06	-66.79	2	16
	q3map_sunExt	0.7451	0.9228	1.0		20.8505	127.32	-86.86	2	16

	q3map_sunExt	0.7415	0.9044	1.0		1.2333	173.79	-18.75	2	16		// the rest of the blue stuff
	q3map_sunExt	0.8389	0.9373	1.0		6.6010	175.55	23.015	2	16
	q3map_sunExt	0.7443	0.8936	1.0		1.8799	243.68	-1.394	2	16
	q3map_sunExt	0.2762	0.6545	1.0		1.4207	221.83	35.078	2	16
	q3map_sunExt	0.7347	0.9285	1.0		0.7111	217.39	-35.32	2	16
	q3map_sunExt	0.6873	0.8762	1.0		4.2276	105.49	2.9141	2	16
	q3map_sunExt	0.8551	0.9596	1.0		1.5355	129.53	-32.12	2	16
	q3map_sunExt	0.6645	0.8419	1.0		5.1380	129.71	38.400	2	16
	q3map_sunExt	0.8921	0.9633	1.0		2.4988	355.55	-23.01	2	16
	q3map_sunExt	0.6714	0.8729	1.0		9.9313	353.79	18.759	2	16
	q3map_sunExt	0.7389	0.8900	1.0		3.5149	63.682	1.3945	2	16
	q3map_sunExt	0.7839	0.9380	1.0		1.0089	41.830	-35.07	2	16
	q3map_sunExt	0.4945	0.7755	1.0		9.5803	37.399	35.329	2	16
	q3map_sunExt	0.7474	0.9022	1.0		2.2217	285.49	-2.914	2	16
	q3map_sunExt	0.6452	0.8375	1.0		6.3072	309.53	32.128	2	16
	q3map_sunExt	0.7980	0.9438	1.0		1.1175	309.71	-38.40	2	16
	q3map_sunExt	0.4813	0.7461	1.0		4.1235	78.366	71.285	2	16
	q3map_sunExt	0.6419	0.8251	1.0		4.6156	269.61	66.695	2	16
	q3map_sunExt	0.7008	0.8735	1.0		2.6714	206.35	0.5979	2	16
	q3map_sunExt	0.6039	0.8304	1.0		2.1635	142.92	3.0233	2	16
	q3map_sunExt	0.7397	0.8970	1.0		0.9118	171.14	-56.08	2	16
	q3map_sunExt	0.8447	0.9514	1.0		1.9548	85.863	-29.38	2	16
	q3map_sunExt	0.7095	0.8839	1.0		0.7305	358.63	-60.33	2	16
	q3map_sunExt	0.6785	0.8827	1.0		0.8571	263.22	-33.99	2	16
	q3map_sunExt	0.6395	0.8322	1.0		4.2932	265.86	29.387	2	16
	q3map_sunExt	0.3589	0.6863	1.0		1.5625	178.63	60.330	2	16
	q3map_sunExt	0.4516	0.7379	1.0		3.8085	83.228	33.998	2	16
	q3map_sunExt	0.7294	0.8934	1.0		5.0831	26.355	-0.597	2	16
	q3map_sunExt	0.6584	0.8480	1.0		9.9217	351.14	56.083	2	16
	q3map_sunExt	0.7483	0.9023	1.0		2.9484	322.92	-3.023	2	16
	q3map_sunExt	0.8120	0.9298	1.0		2.8463	337.34	-12.77	2	16
	q3map_sunExt	0.8911	0.9538	1.0		2.4733	317.59	-19.49	2	16
	q3map_sunExt	0.7197	0.8773	1.0		6.4394	317.26	13.319	2	16
	q3map_sunExt	0.7576	0.9023	1.0		2.6524	305.62	-3.137	2	16
	q3map_sunExt	0.8126	0.9329	1.0		6.0419	336.79	7.3258	2	16
	q3map_sunExt	0.4438	0.7333	1.0		5.8467	327.40	46.881	2	16
	q3map_sunExt	0.4716	0.7394	1.0		4.6595	322.06	66.796	2	16
	q3map_sunExt	0.4634	0.7380	1.0		7.3385	17.011	68.933	2	16
	q3map_sunExt	0.5547	0.8127	1.0		9.9246	352.72	38.834	2	16
	q3map_sunExt	0.8641	0.9603	1.0		9.8788	11.743	8.7114	2	16
	q3map_sunExt	0.6146	0.8392	1.0		9.9495	30.920	16.084	2	16
	q3map_sunExt	0.8098	0.9260	1.0		3.1106	32.759	-16.68	2	16
	q3map_sunExt	0.7695	0.9092	1.0		4.2623	43.610	0.3438	2	16
	q3map_sunExt	0.8011	0.9237	1.0		3.5108	12.751	-11.37	2	16
	q3map_sunExt	0.4877	0.7529	1.0		4.2342	94.591	19.960	2	16
	q3map_sunExt	0.4744	0.7463	1.0		5.6085	73.250	19.172	2	16
	q3map_sunExt	0.5082	0.7608	1.0		4.3806	82.010	51.256	2	16
	q3map_sunExt	0.4573	0.7575	1.0		5.9619	62.266	36.831	2	16
	q3map_sunExt	0.3919	0.6978	1.0		2.8316	103.99	38.348	2	16
	q3map_sunExt	0.4463	0.7303	1.0		2.1759	150.12	52.726	2	16
	q3map_sunExt	0.6964	0.8603	1.0		5.5020	147.14	72.797	2	16
	q3map_sunExt	0.2304	0.6261	1.0		1.2750	204.23	50.626	2	16
	q3map_sunExt	0.2761	0.6490	1.0		1.1326	213.59	70.256	2	16
	q3map_sunExt	0.3160	0.6713	1.0		1.9584	176.73	43.088	2	16
	q3map_sunExt	0.3007	0.6602	1.0		2.1553	246.24	33.984	2	16
	q3map_sunExt	0.5368	0.7670	1.0		3.1211	266.92	46.648	2	16
	q3map_sunExt	0.7343	0.8851	1.0		4.4957	275.63	14.647	2	16
	q3map_sunExt	0.5941	0.8030	1.0		4.6915	285.70	32.546	2	16
	q3map_sunExt	0.7972	0.9108	1.0		5.1333	254.81	15.417	2	16
	q3map_sunExt	0.7461	0.9177	1.0		0.7518	253.25	-19.17	2	16
	q3map_sunExt	0.6744	0.8647	1.0		0.7637	274.59	-19.96	2	16
	q3map_sunExt	0.6557	0.8529	1.0		0.7275	262.01	-51.25	2	16
	q3map_sunExt	0.6262	0.8629	1.0		0.6131	283.99	-38.34	2	16
	q3map_sunExt	0.7284	0.8994	1.0		0.9087	242.26	-36.83	2	16
	q3map_sunExt	0.7288	0.9041	1.0		0.7844	330.12	-52.72	2	16
	q3map_sunExt	0.7232	0.8937	1.0		0.7624	24.233	-50.62	2	16
	q3map_sunExt	0.8388	0.9434	1.0		1.4656	356.73	-43.08	2	16
	q3map_sunExt	0.8740	0.9648	1.0		1.4211	105.70	-32.54	2	16
	q3map_sunExt	0.8685	0.9591	1.0		1.4714	86.920	-46.64	2	16
	q3map_sunExt	0.7767	0.9126	1.0		3.1975	74.818	-15.41	2	16
	q3map_sunExt	0.8519	0.9610	1.0		1.3755	66.249	-33.98	2	16
	q3map_sunExt	0.7512	0.9027	1.0		3.0526	95.635	-14.64	2	16
	q3map_sunExt	0.7048	0.9048	1.0		0.5976	197.12	-48.82	2	16
	q3map_sunExt	0.8587	0.9561	1.0		1.6206	147.40	-46.88	2	16
	q3map_sunExt	0.8380	0.9489	1.0		1.4543	172.72	-38.83	2	16
	q3map_sunExt	0.7567	0.8964	1.0		4.3690	157.34	12.777	2	16
	q3map_sunExt	0.7238	0.8912	1.0		1.9681	156.79	-7.325	2	16
	q3map_sunExt	0.6820	0.8775	1.0		4.0164	125.62	3.1373	2	16
	q3map_sunExt	0.7759	0.9199	1.0		1.5245	137.26	-13.31	2	16
	q3map_sunExt	0.7481	0.8932	1.0		4.8007	137.59	19.499	2	16
	q3map_sunExt	0.6295	0.8308	1.0		4.9848	192.75	11.374	2	16
	q3map_sunExt	0.8258	0.9361	1.0		5.9075	212.75	16.685	2	16
	q3map_sunExt	0.7122	0.8898	1.0		1.4129	210.92	-16.08	2	16
	q3map_sunExt	0.7125	0.8748	1.0		2.8112	223.61	-0.343	2	16
	q3map_sunExt	0.7066	0.8725	1.0		2.0549	191.74	-8.711	2	16
	q3map_sunExt	0.5283	0.7968	1.0		7.0345	332.96	27.179	2	16
	q3map_sunExt	0.7527	0.8984	1.0		3.7793	354.66	-2.128	2	16
	q3map_sunExt	0.6259	0.8739	1.0		9.9599	13.894	28.792	2	16
	q3map_sunExt	0.7860	0.9124	1.0		9.8122	51.895	18.816	2	16
	q3map_sunExt	0.4473	0.7320	1.0		7.5309	48.643	54.730	2	16
	q3map_sunExt	0.7125	0.8651	1.0		6.5596	115.43	57.076	2	16
	q3map_sunExt	0.6884	0.8602	1.0		5.9424	307.32	86.861	2	16
	q3map_sunExt	0.2795	0.6521	1.0		1.4047	236.95	53.050	2	16
	q3map_sunExt	0.5850	0.8002	1.0		4.9306	297.09	50.914	2	16
	q3map_sunExt	0.7501	0.8974	1.0		6.1790	296.51	14.917	2	16
	q3map_sunExt	0.7474	0.8959	1.0		2.3860	264.57	-2.306	2	16
	q3map_sunExt	0.7872	0.9308	1.0		1.0186	296.12	-21.08	2	16
	q3map_sunExt	0.5515	0.8333	1.0		0.3982	295.43	-57.07	2	16
	q3map_sunExt	0.8563	0.9499	1.0		1.4454	334.58	-32.80	2	16
	q3map_sunExt	0.8497	0.9501	1.0		1.6724	17.258	-31.12	2	16
	q3map_sunExt	0.8090	0.9414	1.0		1.1589	56.953	-53.05	2	16
	q3map_sunExt	0.7745	0.9149	1.0		2.4389	53.859	-17.13	2	16
	q3map_sunExt	0.7515	0.8940	1.0		4.1481	84.578	2.3060	2	16
	q3map_sunExt	0.7885	0.9361	1.0		0.9493	228.64	-54.73	2	16
	q3map_sunExt	0.8200	0.9431	1.0		1.1340	117.09	-50.91	2	16
	q3map_sunExt	0.7481	0.8897	1.0		5.5626	154.58	32.801	2	16
	q3map_sunExt	0.8458	0.9499	1.0		1.4160	152.96	-27.17	2	16
	q3map_sunExt	0.8022	0.9261	1.0		1.9189	116.51	-14.91	2	16
	q3map_sunExt	0.5242	0.7729	1.0		4.3637	116.12	21.080	2	16
	q3map_sunExt	0.7076	0.8732	1.0		2.6345	174.66	2.1282	2	16
	q3map_sunExt	0.3855	0.7023	1.0		2.6832	197.25	31.121	2	16
	q3map_sunExt	0.4631	0.7378	1.0		3.8573	233.85	17.131	2	16
	q3map_sunExt	0.7625	0.9123	1.0		1.4190	231.89	-18.81	2	16
	q3map_sunExt	0.7805	0.9345	1.0		1.1819	193.89	-28.79	2	16
}

textures/lun3dm5/hazeb
{
	qer_editorimage textures/lun3dm5/kloudz1.tga
	
	// the fog brush scatters bounce light in q3map2 using the editorimage as lightimage unless this is set :(
	q3map_bounceScale 0.0
	
	surfaceparm	trans
	surfaceparm	nonsolid
	surfaceparm	fog
	surfaceparm	nolightmap
	fogparms ( 0.4 0.6 0.8 ) 12800
}

textures/lun3dm5/c_crete6g
{
	qer_editorimage textures/lun3dm5/c_crete6g.tga
	{
		map textures/lun3dm5/env4.tga
		tcGen environment
	}
	{
		map textures/lun3dm5/c_crete6g.tga
		blendFunc GL_ONE GL_SRC_ALPHA
	}
	{
		map $lightmap
		blendFunc GL_DST_COLOR GL_SRC_COLOR
	}
}

textures/lun3dm5/c_crete6gs
{
	qer_editorimage textures/lun3dm5/c_edges_ed.tga
	{
		map textures/lun3dm5/env4.tga
		tcGen environment
	}
	{
		map textures/lun3dm5/c_crete6g.tga
		tcMod scale 3.9 3.9 	
		blendFunc GL_ONE GL_SRC_ALPHA
	}
	{
		map textures/lun3dm5/c_edges.tga
		blendFunc GL_DST_COLOR GL_SRC_COLOR
	}
	{
		map $lightmap
		blendFunc GL_DST_COLOR GL_SRC_COLOR
	}
}

textures/lun3dm5/c_crete6gsl
{
	qer_editorimage textures/lun3dm5/c_edgesbig_ed.tga
	{
		map textures/lun3dm5/env4.tga
		tcGen environment
	}
	{
		map textures/lun3dm5/c_crete6g.tga
		tcMod scale 7.7 7.7	
		blendFunc GL_ONE GL_SRC_ALPHA
	}
	{
		map textures/lun3dm5/c_edgesbig.tga
		blendFunc GL_DST_COLOR GL_SRC_COLOR
	}
	{
		map $lightmap
		blendFunc GL_DST_COLOR GL_SRC_COLOR
	}
}


textures/lun3dm5/c_crete6gs_hi
{
	q3map_remapShader textures/lun3dm5/c_crete6gs
	qer_editorimage textures/lun3dm5/c_edgeshi_ed.tga
	q3map_lightmapSampleSize 4
}

textures/lun3dm5/c_crete6gs_lo
{
	q3map_remapShader textures/lun3dm5/c_crete6gs
	qer_editorimage textures/lun3dm5/c_edgeslo_ed.tga
	q3map_lightmapSampleSize 16
}

textures/lun3dm5/c_crete6gsl_hi
{
	q3map_remapShader textures/lun3dm5/c_crete6gsl
	qer_editorimage textures/lun3dm5/c_edgesbighi_ed.tga
	q3map_lightmapSampleSize 4
}

textures/lun3dm5/c_crete6gsl_lo
{
	q3map_remapShader textures/lun3dm5/c_crete6gsl
	qer_editorimage textures/lun3dm5/c_edgesbiglo_ed.tga
	q3map_lightmapSampleSize 16
}



textures/lun3dm5/telecone
{
	qer_editorimage textures/lun3dm5/blk1.tga
	surfaceparm nolightmap
	surfaceparm nonsolid
	surfaceparm noimpact
	surfaceparm nomarks
	surfaceparm trans
	cull none
	{
		map textures/lun3dm5/omgstars.tga
		alphaGen vertex
		tcMod scale 2 0.125
		tcMod scroll 0 0.125
		blendFunc GL_SRC_ALPHA GL_ONE
	}
}

textures/lun3dm5/jumpcone
{
	qer_editorimage textures/lun3dm5/blk1.tga
	surfaceparm nolightmap
	surfaceparm nonsolid
	surfaceparm noimpact
	surfaceparm nomarks
	surfaceparm trans
	deformVertexes move 0 0 4 sine 0 1 0 0.25
	{
		map textures/lun3dm5/jfx_dust.tga
		alphaGen vertex
		tcMod scale 2 0.125
		tcMod scroll 0 0.125
		blendFunc GL_SRC_ALPHA GL_ONE_MINUS_SRC_ALPHA
	}
}



textures/lun3dm5/jfx2
{
	qer_editorimage textures/lun3dm5/jfx_puff1.tga
	surfaceparm nonsolid
	surfaceparm nomarks
	surfaceparm noimpact
	surfaceparm nolightmap
	surfaceparm trans
	deformVertexes move 0 0 32 sawtooth 0 1 0 1
	{
		clampMap textures/lun3dm5/jfx_puff1.tga
		rgbGen wave sine 0.15 0.15 0.75 1
		tcMod stretch inverseSawtooth 0.75 0.25 0 1
		blendFunc GL_ONE GL_ONE
	}
	{
		animMap 1 textures/lun3dm5/jfx_l1.tga textures/lun3dm5/jfx_l1.tga textures/lun3dm5/jfx_l2.tga textures/lun3dm5/jfx_l2.tga
		rgbGen wave sine 0.125 0.125 0.75 1
		tcMod stretch inverseSawtooth 1.5 0.5 0 1
		tcMod rotate 17
		blendFunc GL_ONE GL_ONE
	}
	{
		animMap 1 textures/lun3dm5/jfx_r1.tga textures/lun3dm5/jfx_r2.tga textures/lun3dm5/jfx_r2.tga textures/lun3dm5/jfx_r1.tga
		rgbGen wave sine 0.125 0.125 0.75 1
		tcMod stretch inverseSawtooth 1.5 0.5 0 1
		tcMod rotate -14
		blendFunc GL_ONE GL_ONE
	}
}

textures/lun3dm5/tfx2_xp
{
	qer_editorimage textures/lun3dm5/tfx2_1.tga
	surfaceparm nonsolid
	surfaceparm nomarks
	surfaceparm noimpact
	surfaceparm nolightmap
	surfaceparm trans
	deformVertexes move -32 0 0 inversesawtooth 0 -1 0 0.75
	{
		animMap 3 textures/lun3dm5/blk1.tga textures/lun3dm5/tfx2_1.tga textures/lun3dm5/tfx2_2.tga textures/lun3dm5/tfx2_3.tga
		rgbGen wave inverseSawtooth 0 .3 0 3
		tcMod stretch inverseSawtooth 1.5 0.5 0 0.75
		blendFunc GL_ONE GL_ONE
	}
	{
		animMap 3 textures/lun3dm5/tfx2_1.tga textures/lun3dm5/tfx2_2.tga textures/lun3dm5/tfx2_3.tga textures/lun3dm5/blk1.tga
		rgbGen wave sawtooth 0 .3 0 3
		tcMod stretch inverseSawtooth 1.5 0.5 0 0.75
		blendFunc GL_ONE GL_ONE
	}
}

textures/lun3dm5/tfx2_xn
{
	qer_editorimage textures/lun3dm5/tfx2_1.tga
	surfaceparm nonsolid
	surfaceparm nomarks
	surfaceparm noimpact
	surfaceparm nolightmap
	surfaceparm trans
	deformVertexes move 32 0 0 inversesawtooth 0 -1 0 0.75
	{
		animMap 3 textures/lun3dm5/blk1.tga textures/lun3dm5/tfx2_1.tga textures/lun3dm5/tfx2_2.tga textures/lun3dm5/tfx2_3.tga
		rgbGen wave inverseSawtooth 0 .3 0 3
		tcMod stretch inverseSawtooth 1.5 0.5 0 0.75
		blendFunc GL_ONE GL_ONE
	}
	{
		animMap 3 textures/lun3dm5/tfx2_1.tga textures/lun3dm5/tfx2_2.tga textures/lun3dm5/tfx2_3.tga textures/lun3dm5/blk1.tga
		rgbGen wave sawtooth 0 .3 0 3
		tcMod stretch inverseSawtooth 1.5 0.5 0 0.75
		blendFunc GL_ONE GL_ONE
	}
}

textures/lun3dm5/tfx2_yp
{
	qer_editorimage textures/lun3dm5/tfx2_1.tga
	surfaceparm nonsolid
	surfaceparm nomarks
	surfaceparm noimpact
	surfaceparm nolightmap
	surfaceparm trans
	deformVertexes move 0 -32 0 inversesawtooth 0 -1 0 0.75
	{
		animMap 3 textures/lun3dm5/blk1.tga textures/lun3dm5/tfx2_1.tga textures/lun3dm5/tfx2_2.tga textures/lun3dm5/tfx2_3.tga
		rgbGen wave inverseSawtooth 0 .3 0 3
		tcMod stretch inverseSawtooth 1.5 0.5 0 0.75
		blendFunc GL_ONE GL_ONE
	}
	{
		animMap 3 textures/lun3dm5/tfx2_1.tga textures/lun3dm5/tfx2_2.tga textures/lun3dm5/tfx2_3.tga textures/lun3dm5/blk1.tga
		rgbGen wave sawtooth 0 .3 0 3
		tcMod stretch inverseSawtooth 1.5 0.5 0 0.75
		blendFunc GL_ONE GL_ONE
	}
}

textures/lun3dm5/tfx2_yn
{
	qer_editorimage textures/lun3dm5/tfx2_1.tga
	surfaceparm nonsolid
	surfaceparm nomarks
	surfaceparm noimpact
	surfaceparm nolightmap
	surfaceparm trans
	deformVertexes move 0 32 0 inversesawtooth 0 -1 0 0.75
	{
		animMap 3 textures/lun3dm5/blk1.tga textures/lun3dm5/tfx2_1.tga textures/lun3dm5/tfx2_2.tga textures/lun3dm5/tfx2_3.tga
		rgbGen wave inverseSawtooth 0 .3 0 3
		tcMod stretch inverseSawtooth 1.5 0.5 0 0.75
		blendFunc GL_ONE GL_ONE
	}
	{
		animMap 3 textures/lun3dm5/tfx2_1.tga textures/lun3dm5/tfx2_2.tga textures/lun3dm5/tfx2_3.tga textures/lun3dm5/blk1.tga
		rgbGen wave sawtooth 0 .3 0 3
		tcMod stretch inverseSawtooth 1.5 0.5 0 0.75
		blendFunc GL_ONE GL_ONE
	}
}

textures/lun3dm5/d_n
{
	surfaceparm nomarks
	surfaceparm nolightmap
	surfaceparm nonsolid
	surfaceparm noimpact
	surfaceparm trans
	polygonOffset
	{
		map textures/lun3dm5/d_n.tga
		blendFunc GL_DST_COLOR GL_ZERO
	}
}


textures/lun3dm5/c_crete6j
{
	qer_editorimage textures/lun3dm5/c_crete6j.tga
	surfaceparm nodamage
	{
		animMap 3 textures/lun3dm5/jring_1.tga textures/lun3dm5/jring_2.tga textures/lun3dm5/jring_3.tga
		rgbGen wave inverseSawtooth 0 1 0 3
		tcMod stretch sawtooth 0.1 1.5 0 1
	}
	{
		animMap 3 textures/lun3dm5/jring_2.tga textures/lun3dm5/jring_3.tga textures/lun3dm5/blk1.tga
		rgbGen wave sawtooth 0 1 0 3
		tcMod stretch sawtooth 0.1 1.5 0 1
		blendFunc GL_ONE GL_ONE
	}
	{
		clampMap textures/lun3dm5/jring_x.tga
		tcMod stretch sawtooth 0.2 3 0 1
		blendFunc GL_DST_COLOR GL_ZERO
	}
	{
		animMap 1 textures/lun3dm5/jfxmask1.tga textures/lun3dm5/jfxmask2.tga textures/lun3dm5/jfxmask3.tga textures/lun3dm5/jfxmask4.tga
		blendFunc GL_DST_COLOR GL_ZERO
	}
	{
		map textures/lun3dm5/c_crete6j.tga
		blendFunc GL_ONE GL_ONE
	}
	{
		map $lightmap
		blendFunc GL_DST_COLOR GL_SRC_COLOR
	}
}

textures/lun3dm5/c_crete6j_bright
{
	qer_editorimage textures/lun3dm5/c_crete6j.tga
	surfaceparm nodamage
	{
		animMap 3 textures/lun3dm5/jring_1.tga textures/lun3dm5/jring_2.tga textures/lun3dm5/jring_3.tga
		rgbGen wave inverseSawtooth 0 1 0 3
		tcMod stretch sawtooth 0.1 1.5 0 1
	}
	{
		animMap 3 textures/lun3dm5/jring_2.tga textures/lun3dm5/jring_3.tga textures/lun3dm5/blk1.tga
		rgbGen wave sawtooth 0 1 0 3
		tcMod stretch sawtooth 0.1 1.5 0 1
		blendFunc GL_ONE GL_ONE
	}
	{
		clampMap textures/lun3dm5/jring_x.tga
		tcMod stretch sawtooth 0.2 3 0 1
		blendFunc GL_DST_COLOR GL_SRC_COLOR
	}
	{
		animMap 1 textures/lun3dm5/jfxmask1.tga textures/lun3dm5/jfxmask2.tga textures/lun3dm5/jfxmask3.tga textures/lun3dm5/jfxmask4.tga
		blendFunc GL_DST_COLOR GL_ZERO
	}
	{
		map textures/lun3dm5/c_crete6j.tga
		blendFunc GL_ONE GL_ONE
	}
	{
		map $lightmap
		blendFunc GL_DST_COLOR GL_SRC_COLOR
	}
}



textures/lun3dm5/c_crete6t
{
	qer_editorimage textures/lun3dm5/c_crete6t.tga
	surfaceparm nodamage
	{
		animMap 3 textures/lun3dm5/blk1.tga textures/lun3dm5/tring_1.tga textures/lun3dm5/tring_1b.tga textures/lun3dm5/tring_2.tga
		rgbGen wave inverseSawtooth 0 1 0 3
		tcMod stretch inverseSawtooth 0.1 4 0 0.75
	}
	{
		animMap 3 textures/lun3dm5/tring_1.tga textures/lun3dm5/tring_1b.tga textures/lun3dm5/tring_2.tga textures/lun3dm5/tring_3.tga
		rgbGen wave sawtooth 0 1 0 3
		tcMod stretch inverseSawtooth 0.1 4 0 0.75
		blendFunc GL_ONE GL_ONE
	}
	{
		clampMap textures/lun3dm5/tring_x.tga
		tcMod stretch inverseSawtooth 0.2 8 0 0.75
		blendFunc GL_DST_COLOR GL_ZERO
	}
	{
		animMap 0.75 textures/lun3dm5/tfxmask1.tga textures/lun3dm5/tfxmask2.tga textures/lun3dm5/tfxmask3.tga textures/lun3dm5/tfxmask4.tga
		blendFunc GL_DST_COLOR GL_ZERO
	}
	{
		map textures/lun3dm5/c_crete6t.tga
		blendFunc GL_ONE GL_ONE
	}
	{
		map $lightmap
		blendFunc GL_DST_COLOR GL_SRC_COLOR
	}
}

textures/lun3dm5/c_crete6t_bright
{
	qer_editorimage textures/lun3dm5/c_crete6t.tga
	surfaceparm nodamage
	{
		animMap 3 textures/lun3dm5/blk1.tga textures/lun3dm5/tring_1.tga textures/lun3dm5/tring_1b.tga textures/lun3dm5/tring_2.tga
		rgbGen wave inverseSawtooth 0 1 0 3
		tcMod stretch inverseSawtooth 0.1 4 0 0.75
	}
	{
		animMap 3 textures/lun3dm5/tring_1.tga textures/lun3dm5/tring_1b.tga textures/lun3dm5/tring_2.tga textures/lun3dm5/tring_3.tga
		rgbGen wave sawtooth 0 1 0 3
		tcMod stretch inverseSawtooth 0.1 4 0 0.75
		blendFunc GL_ONE GL_ONE
	}
	{
		clampMap textures/lun3dm5/tring_x.tga
		tcMod stretch inverseSawtooth 0.2 8 0 0.75
		blendFunc GL_DST_COLOR GL_ZERO
	}

	{
		animMap 0.75 textures/lun3dm5/tfxmask1.tga textures/lun3dm5/tfxmask2.tga textures/lun3dm5/tfxmask3.tga textures/lun3dm5/tfxmask4.tga
		blendFunc GL_DST_COLOR GL_SRC_COLOR
	}
	{
		map textures/lun3dm5/c_crete6t.tga
		blendFunc GL_ONE GL_ONE
	}
	{
		map $lightmap
		blendFunc GL_DST_COLOR GL_SRC_COLOR
	}
}